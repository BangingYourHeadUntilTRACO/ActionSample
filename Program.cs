using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Data.Entity;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            //AllTheOlderCode();
            AllTheOlderCode_WithAction();
        }

        private static void AllTheOlderCode()
        {
            //XMLWork();
            //QueryXML();

            var cars = ProcessCars("fuel.csv");
            /*
            foreach (var car in cars)
            {
                Console.WriteLine(car.Name);
            }
            */

            var manufacturers = ProcessManufacturers("manufacturers.csv");

            //grouping data to get the top two most fuel efficent models per manufacturer
            var groupQuery =
                from car in cars
                group car by car.Manufacturer.ToUpper() into manufacturer //fix chevy error - one is lower, one is upper
                orderby manufacturer.Key //order by manufacturer so the list makes sense
                select manufacturer; //selecting the output

            foreach (var record in groupQuery)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{record.Key} has {record.Count()} cars");
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var car in record.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("_______________________");
            Console.WriteLine("Example of Grouping in the Method Syntax");
            Console.ForegroundColor = ConsoleColor.White;

            //now do the grouping in the method syntax
            var groupQuery2 =
                cars.GroupBy(c => c.Manufacturer.ToUpper())
                    .OrderBy(g => g.Key);
            foreach (var record in groupQuery2)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{record.Key} has {record.Count()} cars");
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var car in record.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("_______________________");
            Console.WriteLine("Example of Grouping with Join");
            Console.ForegroundColor = ConsoleColor.White;
            var groupjoin =
                from manufacturer in manufacturers
                join car in cars on manufacturer.Name equals car.Manufacturer
                    into carGroup
                orderby manufacturer.Name
                select new
                {
                    Manufacturere = manufacturer,
                    Cars = carGroup
                };

            foreach (var grp in groupjoin)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{grp.Manufacturere} : {grp.Manufacturere.Headquarters}");
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var car in grp.Cars.OrderByDescending(c => c.Combined).Take(2))
                {
                    Console.WriteLine($"\t {car.Name}: {car.Combined}");
                }
            }


            //aggregates
            var AggQuery =
                from car in cars
                group car by car.Manufacturer into carGroup
                select new
                {
                    Name = carGroup.Key,
                    Max = carGroup.Max(c => c.Combined),
                    Min = carGroup.Min(c => c.Combined),
                    Avg = carGroup.Average(c => c.Combined)
                } into reslt
                orderby reslt.Max descending
                select reslt;

            foreach (var rst in AggQuery)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{rst.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\t Max: {rst.Max}");
                Console.WriteLine($"\t Max: {rst.Min}");
                Console.WriteLine($"\t Max: {rst.Avg:N3}");
            }


            //joining two sets of data in linq
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Joining 2 sets of related data using the linq query syntax");
            Console.ForegroundColor = ConsoleColor.White;

            var qJoin =
                from car in cars
                join manufacturer in manufacturers
                    on car.Manufacturer equals manufacturer.Name
                orderby car.Combined descending, car.Name ascending
                select new
                {
                    manufacturer.Headquarters,
                    car.Name,
                    car.Combined
                };


            //join data display
            foreach (var car in qJoin.Take(10))
            {
                Console.WriteLine($"{car.Headquarters} : {car.Name} : {car.Combined}");
            }

            //join in the method syntax
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Joining 2 sets of related data using the linq method syntax");
            Console.ForegroundColor = ConsoleColor.White;
            var qJoin2 =
                cars.Join(manufacturers,
                            c => c.Manufacturer,
                            m => m.Name, (c, m) => new
                            {
                                m.Headquarters,
                                c.Name,
                                c.Combined
                            })
                        .OrderByDescending(c => c.Combined)
                        .ThenBy(c => c.Name);

            foreach (var car in qJoin2.Take(10))
            {
                Console.WriteLine($"{car.Headquarters} : {car.Name} : {car.Combined}");
            }

            //option 2 for joining with linq method syntax - bringing in allfields
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Joining 2 sets of related data using the linq method syntax ALL Data");
            Console.ForegroundColor = ConsoleColor.White;
            var qJoin3 =
                cars.Join(manufacturers,
                            c => c.Manufacturer,
                            m => m.Name, (c, m) => new
                            {
                                Car = c,
                                Manufacturer = m
                            })
                        .OrderByDescending(c => c.Car.Combined)
                        .ThenBy(c => c.Car.Name);

            foreach (var car in qJoin3.Take(10))
            {
                Console.WriteLine(
                    $"{car.Manufacturer.Headquarters} : {car.Car.Name} : {car.Car.Combined}");
            }

            //option 3 for joining with linq method syntax - using two field to join
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Joining 2 sets of related data using the linq Query two field join");
            Console.WriteLine("Composite Join");
            Console.ForegroundColor = ConsoleColor.White;
            var qJoin4 =
                from car in cars
                join manufacturer in manufacturers
                on new { car.Manufacturer, car.Year }
                    equals
                    new { Manufacturer = manufacturer.Name, manufacturer.Year }
                orderby car.Combined descending, car.Name ascending
                select new
                {
                    manufacturer.Headquarters,
                    car.Name,
                    car.Combined
                };

            foreach (var car in qJoin4.Take(10))
            {
                Console.WriteLine(
                    $"{car.Headquarters} : {car.Name} : {car.Combined}");
            }

            //option 4 for joining with linq method syntax - composite 2 field key
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Joining 2 sets of related data using the linq method syntax");
            Console.WriteLine("Composite Joine - two fields");
            Console.ForegroundColor = ConsoleColor.White;
            var qJoin5 =
                cars.Join(manufacturers,
                            c => new { c.Manufacturer, c.Year },
                            m => new { Manufacturer = m.Name, m.Year },
                            (c, m) => new
                            {
                                m.Headquarters,
                                c.Name,
                                c.Combined
                            })
                        .OrderByDescending(c => c.Combined)
                        .ThenBy(c => c.Name);

            foreach (var car in qJoin5.Take(10))
            {
                Console.WriteLine(
                    $"{car.Headquarters} : {car.Name} : {car.Combined}");
            }


            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Filter for the most fuel efficient cars");
            Console.ForegroundColor = ConsoleColor.White;
            //need a secondary sort
            /* extension method syntax
            var query = cars.OrderByDescending(c => c.Combined)
                .ThenBy(c => c.Name); //secondary sort. doing Orderby a second time would overwrite
                                      //the original orderby - which is not the intention
            */
            //using the Query Syntax
            var query =
                from car in cars
                where car.Manufacturer == "BMW" && car.Year == 2016
                orderby car.Combined descending, car.Name ascending
                select car;

            foreach (var car in query.Take(10))
            {
                Console.WriteLine($"{car.Name} : {car.Combined}");
            }

            //no deferred execution in Any<> or All<>
            //however they are as lazy as possible. Once any runs into a single
            //match - it stops looking.
            //once All<> finds something that doesn't match - it returns false
            var AnyCarsByManufacturer = cars.Any(c => c.Manufacturer == "Ford");
            Console.WriteLine();
            Console.WriteLine($"Are there any cars made by Ford { AnyCarsByManufacturer }");

            var AreAllCarsManufactureredByFord = cars.All(c => c.Manufacturer == "Ford");
            Console.WriteLine();
            Console.WriteLine($"Are all cars made by Ford { AreAllCarsManufactureredByFord }");

            //select is the projection operator or transformation
            //good for transformations of data in our query
            var result = cars.Select(c => new { c.Manufacturer, c.Name, c.Combined });
            //the above line creates a new Enumerated List that only has the three elements in it
            //instead of all the elements provided in the original file.

            //SelectMany... Flattens a collection with collections as members into one single
            //flat collection
        }

        private static void AllTheOlderCode_WithAction()
        {
            Action<string> Msg = str => Console.WriteLine(str);
            Action<System.ConsoleColor, string> ConColorW = (clr, str) =>
            {
                Console.ForegroundColor = clr;
                Msg(str);
                Console.ForegroundColor = ConsoleColor.White;
            }; //7 lines
            //XMLWork();
            //QueryXML();

            var cars = ProcessCars("fuel.csv");
            /*
            foreach (var car in cars)
            {
                Console.WriteLine(car.Name);
            }
            */

            var manufacturers = ProcessManufacturers("manufacturers.csv");

            //grouping data to get the top two most fuel efficent models per manufacturer
            var groupQuery =
                from car in cars
                group car by car.Manufacturer.ToUpper() into manufacturer //fix chevy error - one is lower, one is upper
                orderby manufacturer.Key //order by manufacturer so the list makes sense
                select manufacturer; //selecting the output

            foreach (var record in groupQuery)
            {
                ConColorW(ConsoleColor.Green, $"{record.Key} has {record.Count()} cars"); //1
                foreach (var car in record.OrderByDescending(c => c.Combined).Take(2))
                {
                    Msg($"\t{car.Name} : {car.Combined}");//2
                }
            }

            ConColorW(ConsoleColor.Green, "_______________________"); //3
            ConColorW(ConsoleColor.Green, "Example of Grouping in the Method Syntax"); //4

            //now do the grouping in the method syntax
            var groupQuery2 =
                cars.GroupBy(c => c.Manufacturer.ToUpper())
                    .OrderBy(g => g.Key);
            foreach (var record in groupQuery2)
            {
                ConColorW(ConsoleColor.Blue, $"{record.Key} has {record.Count()} cars"); //5
                foreach (var car in record.OrderByDescending(c => c.Combined).Take(2))
                {
                    Msg($"\t{car.Name} : {car.Combined}"); //6
                }
            }

            ConColorW(ConsoleColor.DarkBlue, "_______________________"); //7
            ConColorW(ConsoleColor.DarkBlue, "Example of Grouping with Join"); //8
            var groupjoin =
                from manufacturer in manufacturers
                join car in cars on manufacturer.Name equals car.Manufacturer
                    into carGroup
                orderby manufacturer.Name
                select new
                {
                    Manufacturere = manufacturer,
                    Cars = carGroup
                };

            foreach (var grp in groupjoin)
            {
                ConColorW(ConsoleColor.Red, $"{grp.Manufacturere} : {grp.Manufacturere.Headquarters}"); //9
                foreach (var car in grp.Cars.OrderByDescending(c => c.Combined).Take(2))
                {
                    Msg($"\t {car.Name}: {car.Combined}");//10
                }
            }


            //aggregates
            var AggQuery =
                from car in cars
                group car by car.Manufacturer into carGroup
                select new
                {
                    Name = carGroup.Key,
                    Max = carGroup.Max(c => c.Combined),
                    Min = carGroup.Min(c => c.Combined),
                    Avg = carGroup.Average(c => c.Combined)
                } into reslt
                orderby reslt.Max descending
                select reslt;

            foreach (var rst in AggQuery)
            {
                ConColorW(ConsoleColor.Green, $"{rst.Name}"); //11
                Msg($"\t Max: {rst.Max}"); //12
                Msg($"\t Max: {rst.Min}"); //13
                Msg($"\t Max: {rst.Avg:N3}"); //14
            }


            //joining two sets of data in linq
            ConColorW(ConsoleColor.Blue, "Joining 2 sets of related data using the linq query syntax"); //15

            var qJoin =
                from car in cars
                join manufacturer in manufacturers
                    on car.Manufacturer equals manufacturer.Name
                orderby car.Combined descending, car.Name ascending
                select new
                {
                    manufacturer.Headquarters,
                    car.Name,
                    car.Combined
                };


            //join data display
            foreach (var car in qJoin.Take(10))
            {
                Msg($"{car.Headquarters} : {car.Name} : {car.Combined}"); //16
            }

            //join in the method syntax
            ConColorW(ConsoleColor.Blue, "Joining 2 sets of related data using the linq method syntax"); //17

            var qJoin2 =
                cars.Join(manufacturers,
                            c => c.Manufacturer,
                            m => m.Name, (c, m) => new
                            {
                                m.Headquarters,
                                c.Name,
                                c.Combined
                            })
                        .OrderByDescending(c => c.Combined)
                        .ThenBy(c => c.Name);

            foreach (var car in qJoin2.Take(10))
            {
                Msg($"{car.Headquarters} : {car.Name} : {car.Combined}"); //18
            }

            //option 2 for joining with linq method syntax - bringing in allfields

            ConColorW(ConsoleColor.Blue, "Joining 2 sets of related data using the linq method syntax ALL Data"); //19
            var qJoin3 =
                cars.Join(manufacturers,
                            c => c.Manufacturer,
                            m => m.Name, (c, m) => new
                            {
                                Car = c,
                                Manufacturer = m
                            })
                        .OrderByDescending(c => c.Car.Combined)
                        .ThenBy(c => c.Car.Name);

            foreach (var car in qJoin3.Take(10))
            {
                Msg($"{car.Manufacturer.Headquarters} : {car.Car.Name} : {car.Car.Combined}"); //20
            }

            //option 3 for joining with linq method syntax - using two field to join
            ConColorW(ConsoleColor.Blue, "Joining 2 sets of related data using the linq Query two field join"); //21
            ConColorW(ConsoleColor.Blue, "Composite Join"); //22
            var qJoin4 =
                from car in cars
                join manufacturer in manufacturers
                on new { car.Manufacturer, car.Year }
                    equals
                    new { Manufacturer = manufacturer.Name, manufacturer.Year }
                orderby car.Combined descending, car.Name ascending
                select new
                {
                    manufacturer.Headquarters,
                    car.Name,
                    car.Combined
                };

            foreach (var car in qJoin4.Take(10))
            {
                Msg($"{car.Headquarters} : {car.Name} : {car.Combined}"); //23
            }

            //option 4 for joining with linq method syntax - composite 2 field key
            ConColorW(ConsoleColor.Blue, "Joining 2 sets of related data using the linq method syntax"); //24
            ConColorW(ConsoleColor.Blue, "Composite Join - two fields"); //25
            var qJoin5 =
                cars.Join(manufacturers,
                            c => new { c.Manufacturer, c.Year },
                            m => new { Manufacturer = m.Name, m.Year },
                            (c, m) => new
                            {
                                m.Headquarters,
                                c.Name,
                                c.Combined
                            })
                        .OrderByDescending(c => c.Combined)
                        .ThenBy(c => c.Name);

            foreach (var car in qJoin5.Take(10))
            {
                Msg($"{car.Headquarters} : {car.Name} : {car.Combined}"); //26
            }

            ConColorW(ConsoleColor.DarkBlue, "Filter for the most fuel efficient cars"); //27
            //need a secondary sort
            /* extension method syntax
            var query = cars.OrderByDescending(c => c.Combined)
                .ThenBy(c => c.Name); //secondary sort. doing Orderby a second time would overwrite
                                      //the original orderby - which is not the intention
            */
            //using the Query Syntax
            var query =
                from car in cars
                where car.Manufacturer == "BMW" && car.Year == 2016
                orderby car.Combined descending, car.Name ascending
                select car;

            foreach (var car in query.Take(10))
            {
                Msg($"{car.Name} : {car.Combined}"); //28
            }

            //no deferred execution in Any<> or All<>
            //however they are as lazy as possible. Once any runs into a single
            //match - it stops looking.
            //once All<> finds something that doesn't match - it returns false
            var AnyCarsByManufacturer = cars.Any(c => c.Manufacturer == "Ford");
            Msg(""); //29
            Msg($"Are there any cars made by Ford { AnyCarsByManufacturer }"); //30

            var AreAllCarsManufactureredByFord = cars.All(c => c.Manufacturer == "Ford");
            Msg(""); //31
            Msg($"Are all cars made by Ford { AreAllCarsManufactureredByFord }"); //32

            //select is the projection operator or transformation
            //good for transformations of data in our query
            var result = cars.Select(c => new { c.Manufacturer, c.Name, c.Combined });
            //the above line creates a new Enumerated List that only has the three elements in it
            //instead of all the elements provided in the original file.

            //SelectMany... Flattens a collection with collections as members into one single
            //flat collection
        }

        private static void ActionSample()
        {
            Action<string> Msg = str => Console.WriteLine(str);
            Action<System.ConsoleColor, string> ConGreen = (clr, str) =>
            {
                Console.ForegroundColor = clr;
                Msg(str);
                Console.ForegroundColor = ConsoleColor.White;
            };

        }
        private static void QueryXML()
        {
            var document = XDocument.Load("Fuel.xml");
            var query =
                from element in document.Element("Cars").Elements("Car5")
                //where element.Attribute("Manufacturer")?.Value == "BMW" //questionmark returns null
                //in .Value property if the element doesn't exist in the XML File
                where element.Attribute("Manufacturer").Value == "BMW"
                select element.Attribute("Name").Value;

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }
        }

        public static void XMLWork()
        {
            /*
            var records = ProcessCars("fuel.csv");
            var document = new XDocument();
            var cars = new XElement("Cars");
            foreach (var record in records)
            {
                //better syntax below... commenting out the original
                //var car = new XElement("Car"); //original Car element declaration
                //new declaration below the declaration of the elements.
                //var name = new XElement("Name", record.Name);//original element based
                //var combined = new XElement("Combined", record.Combined); //original element based
                //var name = new XAttribute("Name", record.Name);//original element based
                //var combined = new XAttribute("Combined", record.Combined); //original element based

                /* original code - line of code per element or attribute
                car.Add(name);
                car.Add(combined);
                */

            //var car = new XElement("Car2", name, combined); //2nd variation of creation

            //3rd variation - next one won't even use a for loop
            /*
            var car = new XElement("Car3",
                            new XAttribute("Name", record.Name),
                            new XAttribute("Combined", record.Combined),
                            new XAttribute("Manufacturer", record.Manufacturer)
                            );
            cars.Add(car);
        }
        */
            //using LINQ to work with XML Creation instead of a for loop
            var records = ProcessCars("fuel.csv");
            var document = new XDocument();
            var cars = new XElement("Cars",
                from record in records
                select new XElement("Car5",
                                new XAttribute("Name", record.Name),
                                new XAttribute("Combined", record.Combined),
                                new XAttribute("Manufacturer", record.Manufacturer))
            );
            document.Add(cars);
            document.Save("fuel.xml");
        }
        private static void EndOfTheCourse()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CarDb>());
            //probably wouldn't use the above line of code in production..
            //gives the code permission to wipe out the entire database on changes
            //and then re-create and re-populate it.
            QueryData(); //Query Syntax
            InsertData(); //End of the Course Code
            QueryDataMethodSyntax(); //End of the Course
        }

        private static List<Manufacturer> ProcessManufacturers(string path)
        {
            var query =
                File.ReadAllLines(path)
                    .Where(l => l.Length > 1)
                    .Select(l =>
                    {
                        var columns = l.Split(',');
                        return new Manufacturer
                        {
                            Name = columns[0],
                            Headquarters = columns[1],
                            Year = int.Parse(columns[2])
                        };
                    });
            return query.ToList();
        }

        private static List<Car> ProcessCars(string path)
        {
            return
                File.ReadAllLines(path)
                    .Skip(1) //skips the first line
                    .Where(line => line.Length > 1) //filters out empty lines
                    .Select(Car.ParseFromCsv)
                    .ToList();
        }

        private static List<Car> ProcessFileQuery(string path)
        {
            var query =
                from line in File.ReadAllLines(path).Skip(1)
                where line.Length > 1
                select Car.ParseFromCsv(line);

            return query.ToList();
        }
        private static void QueryDataMethodSyntax()
        {
            var db = new CarDb();

            //database logging
            db.Database.Log = Console.WriteLine;
            var query =
                db.Cars.OrderByDescending(c => c.Combined).ThenBy(c => c.Name).Take(10);

            foreach (var car in query)
            {
                Console.WriteLine($"{car.Name} : {car.Combined}");
            }
        }
        private static void QueryData()
        {
            var db = new CarDb();

            //database logging
            db.Database.Log = Console.WriteLine;

            var query = from car in db.Cars
                        orderby car.Combined descending, car.Name ascending
                        select car;

            foreach (var car in query.Take(10))
            {
                Console.WriteLine($"{car.Name}: {car.Combined}");
            }

        }

        private static void InsertData()
        {
            var cars = ProcessCars("fuel.csv");
            var db = new CarDb();

            //database logging
            db.Database.Log = Console.WriteLine;

            if (!db.Cars.Any())
            {
                foreach (var car in cars)
                {
                    db.Cars.Add(car);
                }
            }
            db.SaveChanges();
        }
    }
}
