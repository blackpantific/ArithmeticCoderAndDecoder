using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArithmeticCoderAndDecoder
{
    class Program
    {
        public static Dictionary<byte, double> AlphabetAndPropabilities { get; set; } = new Dictionary<byte, double>();//алфавит и вероятности
        public static Dictionary<byte, (double, double)> AlphabetProbCumulativeProb { get; set; } = new Dictionary<byte, (double, double)>();//алфавит, вероятности и кумул вероятности
        public static Dictionary<string, (List<byte>, List<byte>)> BlocksAndTheirCodeWords { get; set; }
            = new Dictionary<string, (List<byte>, List<byte>)>();//ключ - суперсимвол(массив байт в стринге), суперсимвол в списке,
        //кодовое слово для суперсимвола в списке
        public static List<byte> ListBinaryToTransform { get; set; } = new List<byte>();//список для преобразования расширением BitArray для записи в файл
        public static Encoding enc { get; set; }
        public static Dictionary<string, string> CodewordsAndTheirRepresentation { get; set; } = new Dictionary<string, string>();
        //первый стринг - кодовое слово, второй - его представление в аски, неободим для проверки совпадений в кодовых словах
        public static uint NumberOfBlocksToDecode { get; set; }//количество суперсимволов в тексте, сколько нужно декодировать

        public static Dictionary<List<byte>, int> ListOfBlockWordsAndTheirLenght { get; set; } = new Dictionary<List<byte>, int>();
        public static List<List<byte>> CodeWords { get; set; } = new List<List<byte>>();//матрица код слов
        public static List<List<byte>> Words { get; set; } = new List<List<byte>>();//матрица слов алфавита(блоков суперсимволов)
        public static List<byte> LenghtOfWords { get; set; } = new List<byte>();//длины код слов





        static void Main(string[] args)
        {

            var OutputFileList = new List<byte>();//текст из сжатого файла
            FileStream fileStream;
            FileInfo fileInfo;

            using (fileStream = File.OpenRead(args[0]))
            {
                var fileSize = fileStream.Length;
                byte[] buffer = new byte[fileSize];

                fileInfo = new FileInfo(args[0]);
                fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));
                OutputFileList = new List<byte>(buffer);

            }


            //AlphabetAndPropabilities.Add(49, 0.1);
            //AlphabetAndPropabilities.Add(50, 0.6);
            //AlphabetAndPropabilities.Add(51, 0.3);
            ArithmeticCoding(OutputFileList, Convert.ToInt32(args[1]), args[2]);

            using (fileStream = File.OpenRead(args[2]))
            {
                var fileSize = fileStream.Length;
                byte[] buffer = new byte[fileSize];

                fileInfo = new FileInfo(args[0]);
                fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));
                OutputFileList = new List<byte>(buffer);

            }


            ArithmeticDecoding(OutputFileList, args[0]);


        }

        public static void ArithmeticDecoding(List<byte> InputFileList, string pathToWrite)
        {
            var blockSize = InputFileList[0];//размер блока суперсимвола
            var nAlphabetSymbols = BitConverter.ToUInt32(InputFileList.GetRange(1, 4).ToArray(), 0);//количество букв(суперсимволов) в алфавите
            var nBlocksToDecode = BitConverter.ToUInt32(InputFileList.GetRange(5, 4).ToArray(), 0);//количество блоков для декодирования

            InputFileList.RemoveRange(0, 9);//удаляем первые 9 информационных символов. остается алфивит с длинами, код слова и код

            for (int i = 0, j = 0; i < nAlphabetSymbols; i++)
            {
                if(i+1 == nAlphabetSymbols)
                {
                    var lastBlockLenght = InputFileList[j];
                    var block = InputFileList.GetRange(j + 1, lastBlockLenght);
                    Words.Add(block);
                    var size = InputFileList[j + 1 + block.Count];
                    LenghtOfWords.Add(size);
                }
                else
                {
                    var block = InputFileList.GetRange(j, blockSize);
                    Words.Add(block);
                    var size = InputFileList[j + blockSize];
                    LenghtOfWords.Add(size);
                    j += blockSize + 1;
                }
            }



        }

        public static List<byte> GettingArithmeticWordsFromData(List<byte> byteArray, int alphabetSize, List<byte> LenghtOfWords)
        {
            int index = 0;
            CodeWords = new List<List<byte>>();
            for (int i = 0; i < alphabetSize; i++)
            {
                CodeWords.Add(new List<byte>());
                CodeWords[i].AddRange(byteArray.GetRange(index, LenghtOfWords[i]));
                index += LenghtOfWords[i];
            }

            byteArray.RemoveRange(0, index);
            return byteArray;
        }


        public static void ArithmeticCoding(List<byte> OutputFileList, int symbInBlock, string pathToWrite)
        {

            ArithmeticCodeBuilder(OutputFileList, Convert.ToInt32(symbInBlock));

            var Output = new List<byte>();
            Output.Add(Convert.ToByte(symbInBlock));//добавляем размер блока, теоретически от 2 до 255

            uint numOfSupersymbols = (uint)BlocksAndTheirCodeWords.Count;
            Output.AddRange(BitConverter.GetBytes(numOfSupersymbols));//количество суперсимволов(букв алфавита)

            //var r = BitConverter.ToUInt32(Output.GetRange(1, 4).ToArray(), 0);

            Output.AddRange(BitConverter.GetBytes(NumberOfBlocksToDecode));//количество суперсимволов в тексте, сколько нужно декодировать

            //var r = BitConverter.ToUInt32(Output.GetRange(5, 4).ToArray(), 0);

            var alphabetRange = new List<byte>();
            for (int i = 0; i < BlocksAndTheirCodeWords.Count; i++)
            {//запись алфавита суперсимволов в список. Каждому суперсимволу в конце добавляется длина его кодового слова.
                // У последнего суперсимвола сначала идет его длина(суперсимвола), затем сам суперсимвол и затем его кодовое слово
                if (i + 1 == BlocksAndTheirCodeWords.Count)
                {
                    alphabetRange.Add((byte)BlocksAndTheirCodeWords.ElementAt(i).Value.Item1.Count);
                    alphabetRange.AddRange(BlocksAndTheirCodeWords.ElementAt(i).Value.Item1);
                    alphabetRange.Add((byte)BlocksAndTheirCodeWords.ElementAt(i).Value.Item2.Count);
                }
                else
                {
                    alphabetRange.AddRange(BlocksAndTheirCodeWords.ElementAt(i).Value.Item1);
                    alphabetRange.Add((byte)BlocksAndTheirCodeWords.ElementAt(i).Value.Item2.Count);
                }

            }

            Output.AddRange(alphabetRange);


            var listOfAllArithmeticWords = new List<byte>();//склеиваем все кодовые слова вместе
            for (int i = 0; i < BlocksAndTheirCodeWords.Count; i++)//добавляем все кодовые слов
            {
                listOfAllArithmeticWords.AddRange(BlocksAndTheirCodeWords.ElementAt(i).Value.Item2);
            }

            listOfAllArithmeticWords.AddRange(ListBinaryToTransform);//склеиваем кодовые слова и текст из кодовых слов для трансформации в битовый массив

            var wordsAfterConvertion = listOfAllArithmeticWords.ToBitArray(listOfAllArithmeticWords.Count);//трансформируем в BitArray
            var byteArrayWords = wordsAfterConvertion.BitArrayToByteArray();//BitArray в массив байт

            //var reverse = byteArrayWords.ByteArrayToBitList();

            Output.AddRange(byteArrayWords);

            using (FileStream fs = File.Create(pathToWrite))
            {
                fs.Write(Output.ToArray(), 0, Output.Count);
            }
        }

        public static void ArithmeticCodeBuilder(List<byte> OutputFileList, int nSymbolsInBlocks)//число от 1 до 255
        {
            enc = Encoding.ASCII;
            GettingAlphabetAndProbabilities(OutputFileList);
            GettingCumulativeProbability(AlphabetAndPropabilities);

            for (int i = 0; i < OutputFileList.Count; i+=nSymbolsInBlocks)
            {
                if((i + nSymbolsInBlocks) > OutputFileList.Count)
                {
                    var list = OutputFileList.GetRange(i, OutputFileList.Count - i);
                    CreatingArithmeticWord(list);
                }
                else
                {
                    var list = OutputFileList.GetRange(i, nSymbolsInBlocks);
                    CreatingArithmeticWord(list);
                }

            }




        }

        public static void CreatingArithmeticWord(List<byte> ListToEncode)
        {
            if (BlocksAndTheirCodeWords.ContainsKey(enc.GetString(ListToEncode.ToArray())))
            {
                var key = enc.GetString(ListToEncode.ToArray());
                ListBinaryToTransform.AddRange(BlocksAndTheirCodeWords[key].Item2);
            }
            else
            {
                var blockCumulativeprobability = GettingCumulativeProbabilityRecursion(ListToEncode, 0, ListToEncode.Count - 1);
                var Gresult = GettingProbabilityRecursion(ListToEncode, 0, ListToEncode.Count - 1);

                var lenghtCodeWord = Math.Ceiling(Math.Abs(Math.Log(Gresult, 2)) + 1);//длина кодового слова

                var wordToConvert = blockCumulativeprobability + Gresult / 2;//кодовое слово в десятичном представлении

                var output = printBinary(wordToConvert);
                var trimmedOutput = output.Substring(0, (int)lenghtCodeWord);

                var codeword = new List<byte>();
                var list = output.ToArray();
                for (int i = 0; i < lenghtCodeWord; i++)
                {
                    var d = list[i].ToString();
                    codeword.Add(Byte.Parse(d));
                }

                //"00001101010110111001110001101100"
                //"00001101010110111001110001101100"
                //"0000110101011011100111000110110001110111000100100001010110000000"
                var m = enc.GetString(ListToEncode.ToArray());


                //сделать проверку на уже существующие слова
                if (CodewordsAndTheirRepresentation.ContainsKey(trimmedOutput))
                {
                    //error
                }
                else
                {
                    CodewordsAndTheirRepresentation.Add(trimmedOutput, enc.GetString(ListToEncode.ToArray()));
                    BlocksAndTheirCodeWords.Add((enc.GetString(ListToEncode.ToArray())), (ListToEncode, codeword));
                    ListBinaryToTransform.AddRange(codeword);
                    NumberOfBlocksToDecode += 1;
                }
            }

        }

        public static String printBinary(double num)
        {
            // Check Number is Between 0 to 1 or Not  
            if (num >= 1 || num <= 0)
                return "ERROR";

            StringBuilder binary = new StringBuilder();
            //binary.Append(".");

            while (num > 0)
            {
                if (binary.Length >= 64)
                    return binary.ToString();

                double r = num * 2;
                if (r >= 1)
                {
                    binary.Append(1);
                    num = r - 1;
                }
                else
                {
                    binary.Append(0);
                    num = r;
                }
            }
            if (binary.Length < 64)
            {
                char[] vs = new char[64 - binary.Length];
                for (int i = 0; i < vs.Length; i++)
                {
                    vs[i] = '0';

                }
                binary.Append(vs);
            }
            return binary.ToString();
        }

        public static void GettingAlphabetAndProbabilities(List<byte> byteArray)
        {
            var numberOfSymbolsInArray = byteArray.Count;
            double countSymbols;//сколько раз встречается тот или иной элемент
            double propability;
            for (int i = 0; i < numberOfSymbolsInArray; i++)
            {
                if (AlphabetAndPropabilities.ContainsKey(byteArray[i]))
                {
                    continue;
                }
                else
                {
                    countSymbols = byteArray.Where(x => x.Equals(byteArray[i])).Count();
                    propability = countSymbols / numberOfSymbolsInArray;
                    AlphabetAndPropabilities.Add(byteArray[i], propability);
                }
            }
        }

        public static void GettingCumulativeProbability(Dictionary<byte, double> alphAndProb)
        {
            var sorted = alphAndProb.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);

            for (int i = 0; i < sorted.Count; i++)
            {
                if (i == 0)
                {
                    //sorted.ElementAt(i)
                    var tuple = (sorted.ElementAt(i).Key, sorted.ElementAt(i).Value);
                    AlphabetProbCumulativeProb.Add(sorted.ElementAt(i).Key, (sorted.ElementAt(i).Value, 0.0));
                }
                else
                {

                    var cumulatProp = AlphabetProbCumulativeProb.ElementAt(i - 1).Value.Item1
                        + AlphabetProbCumulativeProb.ElementAt(i - 1).Value.Item2;

                    AlphabetProbCumulativeProb.Add(sorted.ElementAt(i).Key, (sorted.ElementAt(i).Value, cumulatProp));
                }
            }
        }

        public static double GettingProbabilityRecursion(List<byte> byteArray, int firstPosition, int secondPosition)
        {
            if (secondPosition == firstPosition)
            {
                var a = AlphabetAndPropabilities[byteArray[secondPosition]];
                return a;
            }
            else
            {
                var a = GettingProbabilityRecursion(byteArray, firstPosition, secondPosition - 1) *
                    AlphabetAndPropabilities[byteArray[secondPosition]];
                return a;
            }
        }

        public static double GettingCumulativeProbabilityRecursion(List<byte> byteArray, int firstPosition, int secondPosition)
        {
            if (secondPosition == firstPosition)
            {
                var a = AlphabetProbCumulativeProb[byteArray[secondPosition]].Item2;
                return a;
            }
            else
            {
                var a = GettingCumulativeProbabilityRecursion(byteArray, firstPosition, secondPosition - 1)
                    + GettingProbabilityRecursion(byteArray, firstPosition, secondPosition - 1)
                    * AlphabetProbCumulativeProb[byteArray[secondPosition]].Item2;
                return a;
            }
        }


    }
}
