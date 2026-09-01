using System;
using System.Threading.Tasks;
using Financiera.Services;

namespace Financiera
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bankingService = new BankingService();
            bool keepRunning = true;

            while (keepRunning)
            {
                Console.Clear();
                Console.WriteLine("=== COOPERATIVA FINANCIERA EL PROGRESO ===");
                Console.WriteLine("1. Registrar asociado nuevo");
                Console.WriteLine("2. Listar todos los asociados");
                Console.WriteLine("3. Buscar asociado por número de documento");
                Console.WriteLine("4. Buscar asociado por nombre");
                Console.WriteLine("5. Actualizar datos de un asociado");
                Console.WriteLine("6. Eliminar un asociado");
                Console.WriteLine("7. Consultar saldo de un asociado");
                Console.WriteLine("8. Consultar saldo convertido a dólares (TRM Oficial)");
                Console.WriteLine("9. Registrar consignación");
                Console.WriteLine("10. Registrar retiro");
                Console.WriteLine("11. Ver movimientos de un asociado");
                Console.WriteLine("12. Informes de gerencia");
                Console.WriteLine("0. Salir");
                Console.Write("\nSeleccione una opción: ");

                string? option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Console.Write("Número de documento: "); string doc = Console.ReadLine() ?? "";
                        Console.Write("Nombre completo: "); string name = Console.ReadLine() ?? "";
                        Console.Write("Teléfono: "); string phone = Console.ReadLine() ?? "";
                        Console.Write("Dirección: "); string address = Console.ReadLine() ?? "";
                        Console.WriteLine(bankingService.RegisterAssociated(doc, name, phone, address));
                        break;

                    case "2":
                        var list = bankingService.ListAllAssociateds();
                        Console.WriteLine("\n--- LISTA DE ASOCIADOS ---");
                        foreach (var a in list)
                        {
                            Console.WriteLine($"Doc: {a.DocumentNumber} | Nombre: {a.FullName} | Tel: {a.Phone}");
                        }
                        break;

                    case "3":
                        Console.Write("Número de documento a buscar: "); string searchDoc = Console.ReadLine() ?? "";
                        var found = bankingService.SearchByDocument(searchDoc);
                        if (found != null)
                            Console.WriteLine($"Encontrado -> Doc: {found.DocumentNumber}, Nombre: {found.FullName}, Tel: {found.Phone}, Dirección: {found.Address}");
                        else
                            Console.WriteLine("Asociado no encontrado.");
                        break;

                    case "4":
                        Console.Write("Nombre o parte del nombre a buscar: "); string queryName = Console.ReadLine() ?? "";
                        var matches = bankingService.SearchByName(queryName);
                        Console.WriteLine($"\n--- RESULTADOS DE BÚSQUEDA ({matches.Count}) ---");
                        foreach (var m in matches)
                        {
                            Console.WriteLine($"Doc: {m.DocumentNumber} | Nombre: {m.FullName}");
                        }
                        break;

                    case "5":
                        Console.Write("Número de documento del asociado a actualizar: "); string updDoc = Console.ReadLine() ?? "";
                        Console.Write("Nuevo nombre completo: "); string updName = Console.ReadLine() ?? "";
                        Console.Write("Nuevo teléfono: "); string updPhone = Console.ReadLine() ?? "";
                        Console.Write("Nueva dirección: "); string updAddress = Console.ReadLine() ?? "";
                        Console.WriteLine(bankingService.UpdateAssociated(updDoc, updName, updPhone, updAddress));
                        break;

                    case "6":
                        Console.Write("Número de documento del asociado a eliminar: "); string delDoc = Console.ReadLine() ?? "";
                        Console.WriteLine(bankingService.DeleteAssociated(delDoc));
                        break;

                    case "7":
                        Console.Write("Número de documento del asociado: "); string balDoc = Console.ReadLine() ?? "";
                        decimal balance = bankingService.CalculateBalance(balDoc);
                        Console.WriteLine($"El saldo actual del asociado es: {balance:C}");
                        break;

                    case "8":
                        Console.Write("Número de documento del asociado: "); string usdDoc = Console.ReadLine() ?? "";
                        var (resultText, errorMsg) = await bankingService.GetBalanceInDollarsAsync(usdDoc);
                        if (!string.IsNullOrEmpty(errorMsg))
                            Console.WriteLine($"Aviso: {errorMsg}");
                        else
                            Console.WriteLine($"\n{resultText}");
                        break;

                    case "9":
                        Console.Write("Número de documento del asociado: "); string depDoc = Console.ReadLine() ?? "";
                        Console.Write("Valor de la consignación: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal depAmount))
                            Console.WriteLine(bankingService.RegisterDeposit(depDoc, depAmount));
                        else
                            Console.WriteLine("Valor inválido.");
                        break;

                    case "10":
                        Console.Write("Número de documento del asociado: "); string witDoc = Console.ReadLine() ?? "";
                        Console.Write("Valor del retiro: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal witAmount))
                            Console.WriteLine(bankingService.RegisterWithdrawal(witDoc, witAmount));
                        else
                            Console.WriteLine("Valor inválido.");
                        break;

                    case "11":
                        Console.Write("Número de documento del asociado: "); string movDoc = Console.ReadLine() ?? "";
                        var movements = bankingService.GetMovements(movDoc);
                        Console.WriteLine($"\n--- MOVIMIENTOS ---");
                        foreach (var m in movements)
                        {
                            Console.WriteLine($"Fecha: {m.Date:yyyy-MM-dd HH:mm} | Tipo: {m.Type} | Valor: {m.Amount:C} | Comisión: {m.Commission:C}");
                        }
                        break;

                    case "12":
                        ShowReportsMenu(bankingService);
                        break;

                    case "0":
                        keepRunning = false;
                        break;

                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }

                if (keepRunning)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowReportsMenu(BankingService bankingService)
        {
            Console.Clear();
            Console.WriteLine("=== INFORMES DE GERENCIA ===");
            Console.WriteLine("1. ¿Cuánta plata tenemos?");
            Console.WriteLine("2. ¿Quiénes son mis mejores asociados?");
            Console.WriteLine("3. ¿Quiénes están dormidos?");
            Console.WriteLine("4. ¿Cómo nos fue en un periodo?");
            Console.WriteLine("5. ¿Cuáles fueron los movimientos más grandes?");
            Console.WriteLine("6. ¿Quién me está moviendo la caja?");
            Console.Write("Seleccione el informe: ");

            string? repOption = Console.ReadLine();
            switch (repOption)
            {
                case "1":
                    Console.WriteLine("\n" + bankingService.GetReport1TotalMoney());
                    break;
                case "2":
                    Console.WriteLine("\n" + bankingService.GetReport2TopAssociateds());
                    break;
                case "3":
                    Console.WriteLine("\n" + bankingService.GetReport3SleepingAssociateds());
                    break;
                case "4":
                    Console.Write("Fecha inicial (YYYY-MM-DD): ");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime start))
                    {
                        Console.Write("Fecha final (YYYY-MM-DD): ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime end))
                        {
                            Console.WriteLine("\n" + bankingService.GetReport4PeriodSummary(start, end));
                        }
                    }
                    break;
                case "5":
                    Console.WriteLine("\n" + bankingService.GetReport5LargestMovements());
                    break;
                case "6":
                    Console.WriteLine("\n" + bankingService.GetReport6MovementActivity());
                    break;
                default:
                    Console.WriteLine("Informe no válido.");
                    break;
            }
        }
    }
}