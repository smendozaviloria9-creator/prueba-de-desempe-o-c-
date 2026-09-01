using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financiera.Data;
using Financiera.Data;
using financiera.Models;
using Financiera.Models;

namespace Financiera.Services
{
    public class BankingService
    {
        private readonly AssociatedRepository _associatedRepository = new AssociatedRepository();
        private readonly MovementRepository _movementRepository = new MovementRepository();
        private readonly ExchangeRateService _exchangeRateService = new ExchangeRateService();

        public string RegisterAssociated(string document, string name, string phone, string address)
        {
            if (string.IsNullOrWhiteSpace(document) || string.IsNullOrWhiteSpace(name))
                return "El documento y el nombre son obligatorios.";

            if (_associatedRepository.GetByDocument(document) != null)
                return "Ya existe un asociado registrado con ese número de documento.";

            var newAssociated = new Associated
            {
                DocumentNumber = document,
                FullName = name,
                Phone = phone,
                Address = address
            };

            _associatedRepository.Insert(newAssociated);
            return "Asociado registrado exitosamente con cuenta de ahorros en cero.";
        }

        public List<Associated> ListAllAssociateds() => _associatedRepository.GetAll();

        public Associated? SearchByDocument(string document) => _associatedRepository.GetByDocument(document);

        public List<Associated> SearchByName(string name) => _associatedRepository.GetByName(name);

        public string UpdateAssociated(string document, string name, string phone, string address)
        {
            var associated = _associatedRepository.GetByDocument(document);
            if (associated == null) return "Asociado no encontrado.";

            associated.FullName = name;
            associated.Phone = phone;
            associated.Address = address;
            _associatedRepository.Update(associated);
            return "Datos actualizados correctamente.";
        }

        public string DeleteAssociated(string document)
        {
            var associated = _associatedRepository.GetByDocument(document);
            if (associated == null) return "Asociado no encontrado.";

            var movements = _movementRepository.GetByDocument(document);
            decimal balance = CalculateBalance(document);

            if (balance > 0 || movements.Count > 0)
                return "No se puede eliminar un asociado que tenga saldo o movimientos registrados.";

            _associatedRepository.Dlete(document);
            return "Asociado eliminado lógicamente.";
        }

        public decimal CalculateBalance(string document)
        {
            var movements = _movementRepository.GetByDocument(document);
            decimal balance = 0;
            foreach (var m in movements)
            {
                if (m.Type == MovementType.Deposit)
                    balance += m.Amount;
                else if (m.Type == MovementType.Withdrawal)
                    balance -= (m.Amount + m.Commission);
            }
            return balance;
        }

        public string RegisterDeposit(string document, decimal amount)
        {
            if (amount <= 0) return "El valor del movimiento debe ser mayor a cero.";
            var associated = _associatedRepository.GetByDocument(document);
            if (associated == null) return "Asociado no encontrado.";

            var movement = new Movement
            {
                DocumentNumber = document,
                Type = MovementType.Deposit,
                Amount = amount,
                Commission = 0,
                Date = DateTime.Now
            };

            _movementRepository.Insert(movement);
            return $"Consignación registrada con éxito. Nuevo saldo: {CalculateBalance(document):C}";
        }

        public string RegisterWithdrawal(string document, decimal amount)
        {
            if (amount <= 0) return "El valor del movimiento debe ser mayor a cero.";
            var associated = _associatedRepository.GetByDocument(document);
            if (associated == null) return "Asociado no encontrado.";

            decimal commission = amount > 1000000 ? 8000m : 0m;
            decimal currentBalance = CalculateBalance(document);
            decimal totalDeduction = amount + commission;

            if (currentBalance < totalDeduction)
                return $"Fondos insuficientes. El saldo actual ({currentBalance:C}) no cubre el retiro más la comisión de manejo ({commission:C}).";

            var movement = new Movement
            {
                DocumentNumber = document,
                Type = MovementType.Withdrawal,
                Amount = amount,
                Commission = commission,
                Date = DateTime.Now
            };

            _movementRepository.Insert(movement);
            return $"Retiro registrado con éxito. Comisión aplicada: {commission:C}. Nuevo saldo: {CalculateBalance(document):C}";
        }

        public List<Movement> GetMovements(string document) => _movementRepository.GetByDocument(document);

        public async Task<(string reportText, string error)> GetBalanceInDollarsAsync(string document)
        {
            var associated = _associatedRepository.GetByDocument(document);
            if (associated == null) return (string.Empty, "Asociado no encontrado.");

            decimal balanceCop = CalculateBalance(document);
            var (trm, error) = await _exchangeRateService.FetchCurrentTrmAsync();

            if (trm == null || !decimal.TryParse(trm.Value.Replace(",", "."), out decimal trmValue) || trmValue <= 0)
            {
                return (string.Empty, "No fue posible obtener la tasa oficial del sistema. Operando con normalidad, pero sin conversión disponible.");
            }

            decimal balanceUsd = balanceCop / trmValue;
            string result = $"Saldo en COP: {balanceCop:C}\nSaldo en USD: {balanceUsd:N2} USD\nTRM Oficial utilizada: {trmValue:C} (Vigencia: {trm.ValidFrom} a {trm.ValidUntil})";
            return (result, string.Empty);
        }

        // Informes de gerencia
        public string GetReport1TotalMoney()
        {
            var associateds = _associatedRepository.GetAll();
            int totalAssociateds = associateds.Count;
            decimal totalMoney = associateds.Sum(a => CalculateBalance(a.DocumentNumber));
            decimal avgMoney = totalAssociateds > 0 ? totalMoney / totalAssociateds : 0;

            return $"--- INFORME 1: ¿Cuánta plata tenemos? ---\n" +
                   $"Saldo total de la cooperativa: {totalMoney:C}\n" +
                   $"Cantidad de asociados: {totalAssociateds}\n" +
                   $"Saldo promedio por asociado: {avgMoney:C}";
        }

        public string GetReport2TopAssociateds()
        {
            var associateds = _associatedRepository.GetAll();
            var top = associateds
                .Select(a => new { Associated = a, Balance = CalculateBalance(a.DocumentNumber) })
                .OrderByDescending(x => x.Balance)
                .Take(5)
                .ToList();

            string report = "--- INFORME 2: ¿Quiénes son mis mejores asociados? ---\n";
            int rank = 1;
            foreach (var item in top)
            {
                report += $"{rank}. Doc: {item.Associated.DocumentNumber} | Nombre: {item.Associated.FullName} | Saldo: {item.Balance:C}\n";
                rank++;
            }
            return report;
        }

        public string GetReport3SleepingAssociateds()
        {
            var associateds = _associatedRepository.GetAll();
            var movements = _movementRepository.GetAll();
            var sleeping = associateds
                .Where(a => !movements.Any(m => m.DocumentNumber == a.DocumentNumber))
                .ToList();

            string report = "--- INFORME 3: ¿Quiénes están dormidos? ---\n" +
                            $"Total asociados sin movimientos: {sleeping.Count}\n";
            foreach (var s in sleeping)
            {
                report += $"- Doc: {s.DocumentNumber} | Nombre: {s.FullName}\n";
            }
            return report;
        }

        public string GetReport4PeriodSummary(DateTime startDate, DateTime endDate)
        {
            var movements = _movementRepository.GetAll()
                .Where(m => m.Date.Date >= startDate.Date && m.Date.Date <= endDate.Date)
                .ToList();

            decimal totalDeposits = movements.Where(m => m.Type == MovementType.Deposit).Sum(m => m.Amount);
            int countDeposits = movements.Count(m => m.Type == MovementType.Deposit);

            decimal totalWithdrawals = movements.Where(m => m.Type == MovementType.Withdrawal).Sum(m => m.Amount + m.Commission);
            int countWithdrawals = movements.Count(m => m.Type == MovementType.Withdrawal);

            decimal difference = totalDeposits - totalWithdrawals;

            return $"--- INFORME 4: ¿Cómo nos fue en el periodo ({startDate:yyyy-MM-dd} al {endDate:yyyy-MM-dd})? ---\n" +
                   $"Entró en consignaciones: {totalDeposits:C} ({countDeposits} movimientos)\n" +
                   $"Salió en retiros (incluyendo comisiones): {totalWithdrawals:C} ({countWithdrawals} movimientos)\n" +
                   $"Diferencia (Entrada - Salida): {difference:C}";
        }

        public string GetReport5LargestMovements()
        {
            var allMovements = _movementRepository.GetAll()
                .OrderByDescending(m => m.Amount + m.Commission)
                .Take(10)
                .ToList();

            string report = "--- INFORME 5: ¿Cuáles fueron los movimientos más grandes? ---\n";
            foreach (var m in allMovements)
            {
                var associated = _associatedRepository.GetByDocument(m.DocumentNumber);
                string name = associated?.FullName ?? "Desconocido";
                decimal totalVal = m.Amount + m.Commission;
                report += $"- Fecha: {m.Date:yyyy-MM-dd HH:mm} | Tipo: {m.Type} | Valor Total: {totalVal:C} | Asociado: {name}\n";
            }
            return report;
        }

        public string GetReport6MovementActivity()
        {
            var associateds = _associatedRepository.GetAll();
            var summary = associateds.Select(a =>
            {
                var movs = _movementRepository.GetByDocument(a.DocumentNumber);
                return new
                {
                    Associated = a,
                    Count = movs.Count,
                    TotalDeposited = movs.Where(m => m.Type == MovementType.Deposit).Sum(m => m.Amount),
                    TotalWithdrawn = movs.Where(m => m.Type == MovementType.Withdrawal).Sum(m => m.Amount + m.Commission),
                    CurrentBalance = CalculateBalance(a.DocumentNumber)
                };
            }).OrderByDescending(x => x.Count).ToList();

            string report = "--- INFORME 6: ¿Quién me está moviendo la caja? ---\n";
            foreach (var item in summary)
            {
                report += $"Nombre: {item.Associated.FullName} | Movimientos: {item.Count} | Consignado: {item.TotalDeposited:C} | Retirado: {item.TotalWithdrawn:C} | Saldo: {item.CurrentBalance:C}\n";
            }
            return report;
        }
    }
}