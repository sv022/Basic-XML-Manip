using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;



static string Input(string message){
    Console.WriteLine();
    Console.BackgroundColor = ConsoleColor.DarkGreen;
    Console.Write(message);
    Console.BackgroundColor = ConsoleColor.Black;
    string? value = Console.ReadLine();
    Console.WriteLine();
    return value;
}

static void PrintXMLFile(XDocument Doc) {
    XmlElement? xRoot = DocumentExtensions.ToXmlDocument(Doc).DocumentElement;
    if (xRoot != null) {
        foreach (XmlElement xnode in xRoot) {
            PrintEmployeeXML(xnode, false);
        }
    }
};

static void PrintEmployeeXML(XmlNode employee, bool ShowSalary){
    XElement xEmployee = XElement.Load(employee.CreateNavigator().ReadSubtree());
    Console.WriteLine($"ФИО: {xEmployee.Element("name")?.Value}");
    Console.WriteLine($"Дата рождения: {xEmployee.Element("birthdate")?.Value}");
    Console.WriteLine();
    foreach (XElement job in xEmployee.Element("jobs").Elements().OrderBy(j => j.Element("start")?.Value)) {
        Console.WriteLine($"Должность: {job.Element("position")?.Value}");
        Console.WriteLine($"Поступление: {job.Element("start")?.Value}");
        Console.WriteLine($"Увольнение: {job.Element("end")?.Value}");
        Console.WriteLine($"Отдел: {job.Element("department")?.Value}");
        Console.WriteLine();
    }
    Console.WriteLine();
    if (!ShowSalary) {Console.WriteLine(); return;}
    
    var SortedSalary = xEmployee.Element("salaries").Elements().OrderBy(s => s.Element("year")?.Value).ThenBy(s => s.Element("month")?.Value);
    foreach (XElement salary in SortedSalary){
        Console.WriteLine($"Год: {salary.Element("year")?.Value}");
        Console.WriteLine($"Месяц: {salary.Element("month")?.Value}");
        Console.WriteLine($"Итого: {salary.Element("total")?.Value}");
        Console.WriteLine();
    }

    Console.WriteLine();
    Console.WriteLine($"Максимум: {SortedSalary.Max(s => s.Element("total")?.Value)}");
    Console.WriteLine($"Минимум: {SortedSalary.Min(s => s.Element("total")?.Value)}");
    Console.WriteLine($"Среднее Значение: {SortedSalary.Sum(s => Int32.Parse(s.Element("total")?.Value)) / SortedSalary.Count()}");
    Console.WriteLine();
}

static SortedDictionary<string, int> CountEmployees(XElement xRootc){
    SortedDictionary<string, int> workersCount = new SortedDictionary<string, int>();
    foreach (XElement person in xRootc.Elements("person")){
        foreach (XElement jobEntry in person.Element("jobs").Elements()){
            if (jobEntry.Element("end")?.Value == "-"){
                try {
                    workersCount.Add(jobEntry.Element("department").Value, 1);
                } catch {
                    workersCount[jobEntry.Element("department").Value] += 1;
                }
            }
        }
    }
    return workersCount;
}


static void SearchXML(XDocument Doc) {
    Console.WriteLine("Выберите опцию:\n1 - поиск сотрудника по фамилии\n2 - информация по отделам\n3 - Сотрудники, работающие более чем в одном отделе\n4 - Отделы, в которых работает меньше 3-ех сотрундников\n5 - Статистика HR по годам\n6 - Юбилеи сотрудников");
    string? cmd = Input("Введите команду: ");
    XmlElement? xRoot = DocumentExtensions.ToXmlDocument(Doc).DocumentElement;
    switch (cmd) {
        case "1": {
            string? employee = Input("Введите ФИО сотрудника: ");
            Console.WriteLine();
            XmlNodeList? SearchResults = xRoot?.SelectNodes($"person[name='{employee}']");
            foreach (XmlNode res in SearchResults)
                PrintEmployeeXML(res, true);
            break;
        }
        case "2": {
            string? department = Input("Введите название отдела: ");
            Console.WriteLine();
            int count = 0;
            XmlNodeList? SearchResults = xRoot?.SelectNodes($"person[jobs[job[department='{department}']]]");
            Console.WriteLine($"Должности в отделе {department}: ");
            foreach (XmlNode node in SearchResults) {
                if (node.SelectSingleNode("jobs[job[end='-']]") != null) {
                    count++;
                    XElement xElem = XElement.Load(node.CreateNavigator().ReadSubtree());
                    Console.WriteLine(xElem?.Element("jobs")?.Element("job")?.Element("position")?.Value);
                } 
            }
            Console.WriteLine($"\nВсего сотрудников в отделе {department}: {count}");
            break;
        }
        case "3": {
            XElement xRootc = XElement.Load(xRoot.CreateNavigator().ReadSubtree());
            var res1 = from x in xRootc.Elements("person")
                        where x.Descendants("job").Where(j => j.Element("end")?.Value == "-").Count() > 1
                        select x;
            foreach (var n in res1) Console.WriteLine(n.Element("name")?.Value);
            break;
        }
        case "4": {
            XElement xRootc = XElement.Load(xRoot.CreateNavigator().ReadSubtree());
            SortedSet<string> departments = new SortedSet<string>();
            foreach (XElement person in xRootc.Elements("person")) {
                foreach (XElement job in person.Element("jobs").Elements("job")) departments.Add(job.Element("department")?.Value); //departments.Append(job.Element("department")?.Value)
            }
            //foreach (var d in departments) Console.WriteLine(d);
            var workersCount = CountEmployees(xRootc);
            foreach (KeyValuePair<string, int> kv in workersCount) 
                if (kv.Value <= 3)
                    Console.WriteLine($"{kv.Key}: {kv.Value}");
            break;
        }
        case "5": {
            XElement xRootc = XElement.Load(xRoot.CreateNavigator().ReadSubtree());
            SortedDictionary<int, int> startCount = new SortedDictionary<int, int>();
            SortedDictionary<int, int> endCount = new SortedDictionary<int, int>();
            
            foreach (XElement person in xRootc.Elements("person")){
                foreach (XElement jobEntry in person.Element("jobs").Elements()){
                    var ParsedYear = DateTime.Parse(jobEntry.Element("start")?.Value).Year;
                    try {
                        //Console.WriteLine(ParsedYear.ToString());
                        startCount.Add(ParsedYear, 1);
                    } catch {
                        startCount[ParsedYear] += 1;
                    }
                    if (jobEntry.Element("end")?.Value != "-"){
                        ParsedYear = DateTime.Parse(jobEntry.Element("end")?.Value).Year;
                        try {
                            //Console.WriteLine(ParsedYear.ToString());
                            endCount.Add(ParsedYear, 1);
                        } catch {
                            endCount[ParsedYear] += 1;
                        }
                    }
                }
            }
            Console.WriteLine("Принятые сотрудники:");
            foreach (KeyValuePair<int, int> kv in startCount)
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            Console.WriteLine();
            Console.WriteLine("Уволенные сотрудники:");
            foreach (KeyValuePair<int, int> kv in endCount)
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            Console.WriteLine();
            break;
        }
        case "6": {
            XElement xRootc = XElement.Load(xRoot.CreateNavigator().ReadSubtree());
            var res = from x in xRootc.Elements("person")
                        where DateTime.Parse(x.Element("birthdate")?.Value).Year % 10 == 3
                        select new {name = x.Element("name")?.Value , year = DateTime.Parse(x.Element("birthdate")?.Value).Year};
            
            foreach (var p in res) Console.WriteLine($"{p.name}: {2023 - p.year}");
            Console.WriteLine();
            break;
        }
    }
};

static void ExportXMLData(XDocument Doc){
    XElement xRootc = Doc.Root;
    SortedDictionary<string, int> departments = CountEmployees(xRootc);
    XElement xDepartRoot = new XElement("departments");
    foreach(KeyValuePair<string, int> kv in departments){
        XElement department = new XElement("department",
            new XAttribute("name", kv.Key),
            new XElement("employees", kv.Value.ToString()),
            new XElement("employeesyoung", (kv.Value - (kv.Value / 10)).ToString())
        );
        xDepartRoot.Add(department);
    }
    XDocument newDoc = new XDocument(
        new XDeclaration("1.0", "utf-8", "yes"),
        xDepartRoot
    );
    newDoc.Save(Environment.CurrentDirectory + "/report.xml");
    Console.WriteLine($"Документ сохранен по адресу {Environment.CurrentDirectory}/report.xml.");
}



static XDocument EditXML(XDocument Doc){
    XElement xRootc = Doc.Root;
    Console.WriteLine("1 - Добавить сотрудника\n2 - Удалить сотрудника\n3 - Изменение данных сотрудника");
    string? cmd = Input("Введите команду: ");
    switch (cmd) {
        case "1": {
            string? name = Input("Введите ФИО сотрудника: ");
            string? birthdate = Input("Введите дату рождения сотрудника (формат хх.хх.хххх): ");
            int jobscount = Int32.Parse(Input("Введите ФИО сотрудника: "));
            XElement jobrecords = new XElement("jobs");
            for (int i = 0; i < jobscount; i++){
                Console.WriteLine($"Работа {i + 1}");
                string? position = Input("Должность: ");
                string? start = Input("Дата поступления: ");
                string? end = Input("Дата увольнения: ");
                string? department = Input("Отдел: ");
                XElement job = new XElement("job",
                    new XElement("position", position),
                    new XElement("start", start),
                    new XElement("end", end),
                    new XElement("department", department)
                );
                jobrecords.Add(job);
            }
            XElement person = new XElement("person",
                new XElement("name", name),
                new XElement("birthdate", birthdate),
                jobrecords
            );
            xRootc.Add(person);
            return Doc;
        }
        case "2": {
            string? name = Input("Введите ФИО сотрудника: ");
            xRootc.Descendants().Where(x => x.Element("name")?.Value == name).Remove();
            Console.WriteLine($"Запить о сотруднике {name} удалена\n");
            return Doc;
        }
        case "3": {
            string? name = Input("Введите ФИО сотрудника: ");
            Console.WriteLine("Выберите параметр для редактирования: (фио, дата рождения, зарплата)");
            string? param = Input("Введите параметр: ");
            switch (param){
                case "фио": {
                    XElement person = xRootc.Descendants().Where(x => x.Element("name")?.Value == name).First();
                    string? change = Input("Введите новое имя: ");
                    person.Element("name").Value = change;
                    return Doc;
                }
                case "дата рождения": {
                    XElement person = xRootc.Descendants().Where(x => x.Element("name")?.Value == name).First();
                    string? change = Input("Введите дату (хх.хх.хххх): ");
                    person.Element("birthdate").Value = change;
                    return Doc;
                }
                case "зарплата": {
                    XElement person = xRootc.Descendants().Where(x => x.Element("name")?.Value == name).First();
                    string? changeyear = Input("Введите год: ");
                    string? changemonth = Input("Введите месяц: ");
                    string? changetotal = Input("Введите сумму: ");
                    
                    person.Element("salaries").Add(new XElement("salary",
                        new XElement("year", changeyear),
                        new XElement("month", changemonth),
                        new XElement("total", changetotal)
                    ));
                    return Doc;
                }
            }
            return Doc;
        }
    }
    return Doc;

}

string? filename;

Console.WriteLine("Добро пожаловать в XMLViewer 0.1!\n\nВыберите один из вариантов начала работы:\n1 - Открыть существующий файл\n2 - Завершение работы");
string? cmd = Input("Введите команду: ");
filename = "test.xml";

XDocument xDoc = XDocument.Load(filename);
do {
    Console.WriteLine("----------------------------------");
    Console.WriteLine("Выберите действие:\n1 - Вывести информацию из файла\n2 - поиск по файлу\n3 - редактировать файл\n4 - Экспорт статистики\n5 - Сохранить и выйти");
    cmd = Input("Введите команду: ");
    switch (cmd) {
        case "1": {
            PrintXMLFile(xDoc);
            break;
        }
        case "2": {
            SearchXML(xDoc);
            break;
        }
        case "3": {
            xDoc = EditXML(xDoc);
            break;
        }
        case "4": {
            ExportXMLData(xDoc);
            break;
        }
    }
} while (cmd != "5");
