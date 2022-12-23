using System;
using System.IO;
using System.Xml;
using System.Linq;

namespace PCMT
{

    class PCMT
    {
        static void Main(string[] args)
        {


            if (args.Length == 0)
            {
                Console.WriteLine("未给出路径，请检查");
                return;
            }

            string path = args[0];

            XMLToCSV XMLToCSV = new XMLToCSV();
            XMLToCSV.XMLConvertCSV(path);

            CSVMerge CSVMerge = new CSVMerge();

            CheckCSVFiles CheckCSVFiles = new CheckCSVFiles();
            bool pass = CheckCSVFiles.CheckCSVFile(path);
            if (pass)
            {
                CSVMerge.Merge(path);
            }

        }
    }
    class XMLToCSV
    {
        public void XMLConvertCSV(string args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("输入文件路径中没有找到任何 XML 文件。");
                return;
            }

            //自动索引参数目录中的所有xml文件，并一一生成CSV
            string[] xmlFiles = Directory.GetFiles(args, "*.xml").Where(file => Path.GetExtension(file) == ".xml").ToArray();

            string outputPath = Path.Combine(args, "csv");
            Directory.CreateDirectory(outputPath);

            foreach (string xmlFile in xmlFiles)
            {
                string xmlFilePath = xmlFile;

                string xmlFileName = Path.GetFileName(xmlFilePath);
                string csvFileName = Path.ChangeExtension(xmlFileName, "csv");


                string csvFilePath = Path.Combine(outputPath, csvFileName);


                if (File.Exists(csvFilePath))
                {
                    File.Delete(csvFilePath);
                }

                XmlTextReader reader = new XmlTextReader(xmlFilePath);
                XmlNodeType lastNodeType = XmlNodeType.None;

                using (StreamWriter sw = new StreamWriter(csvFilePath))
                {
                    sw.WriteLine("Element," + xmlFileName);

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (lastNodeType == XmlNodeType.Element)
                            {
                                sw.WriteLine();
                            }
                            sw.Write(reader.Name + ",");
                            lastNodeType = reader.NodeType;
                        }
                        else if (reader.NodeType == XmlNodeType.Text)
                        {
                            sw.WriteLine(reader.Value);
                            lastNodeType = reader.NodeType;
                        }
                    }

                }
                reader.Close();
            }

        }
    }

    class CSVMerge
    {
        public void Merge(string args)
        {


            Console.Write(args);

            string FilePath = Path.Combine(args, "csv");
            string[] inputFiles = Directory.GetFiles(FilePath, "*.csv").Where(file => Path.GetExtension(file) == ".csv").ToArray();


            // 定义输出文件名

            string outputFile = Path.Combine(FilePath, "output.csv");


            // 从第一个文件中读取第一列和第二列的值
            string[] firstColumnValues = File.ReadLines(inputFiles[0])
                .Select(line => line.Split(','))
                .Select(parts => parts[0])
                .ToArray();

            string[] secondColumnValues = File.ReadLines(inputFiles[0])
                .Select(line => line.Split(','))
                .Select(parts => parts[1])
                .ToArray();

            // 从其余文件中读取第二列的值
            string[][] otherColumnValues = new string[inputFiles.Length - 1][];
            for (int i = 1; i < inputFiles.Length; i++)
            {
                otherColumnValues[i - 1] = File.ReadLines(inputFiles[i])
                    .Select(line => line.Split(','))
                    .Select(parts => parts[1])
                    .ToArray();
            }

            // 创建输出文件的内容
            string[] outputLines = new string[firstColumnValues.Length];
            for (int i = 0; i < firstColumnValues.Length; i++)
            {
                string line = $"{firstColumnValues[i]},{secondColumnValues[i]}";
                foreach (string[] columnValues in otherColumnValues)
                {
                    line += $",{columnValues[i]}";
                }
                outputLines[i] = line;
            }

            // 写入输出文件
            File.WriteAllLines(outputFile, outputLines);
        }
    }

    class CheckCSVFiles
    {
        public bool CheckCSVFile(string args)
        {
            // 获取当前目录下的所有 CSV 文件路径
            string FilePath = Path.Combine(args, "csv");
            string[] filePaths = Directory.GetFiles(FilePath, "*.csv");

            if (filePaths.Length == 0)
            {
                Console.WriteLine("Error: No CSV files found.");
                return false;
            }

            // 读取所有文件，并记录行数
            int[] lineCounts = new int[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                using (StreamReader reader = new StreamReader(filePaths[i]))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineCounts[i]++;
                    }
                }
            }

            // 检查所有文件的行数是否都相同
            bool allSame = lineCounts.All(count => count == lineCounts[0]);

            if (allSame)
            {
                foreach (string filePath in filePaths)
                {
                    Console.WriteLine(filePath);
                }
                return true;
            }
            else
            {
                Console.WriteLine("以下文件行数与其他文件不同，可能测试结果有丢失，请检查后删除这些文件再重复执行：");
                for (int i = 0; i < filePaths.Length; i++)
                {
                    if (lineCounts[i] != lineCounts[0])
                    {
                        Console.WriteLine(filePaths[i]);
                    }
                }
                return false;
            }
        }
    }
}