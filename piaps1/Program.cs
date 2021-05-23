using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace ResRegV1cons
{
    class ResAreBusy : Exception { }
    class ResIdInvalid : Exception { }
    class RecIdInvalid : Exception { }
    class UnRecommended : Exception { }
    class ResIsBusy : Exception { }
    class ResWasFree : Exception { }




    static class SetUp
    {
        public static string Path; //путь к файлу, сохраняющему модель
        private static void ClearModel()
        {
            Console.WriteLine("Укажите количество ресурсов:");
            try
            {
                Model.vRes_s = new string[Convert.ToInt32(Console.ReadLine())];
                for (int i = 0; i < Model.vRes_s.Length; i++) Model.vRes_s[i] = "F";
                Model.CntOfFreeRes = Model.vRes_s.Length; // seting up count of free res = total res
            }
            catch
            {
                Console.WriteLine("Введено некорректное число!");
                ClearModel();
            }
        }
        private static void GetModel()
        {
            Console.WriteLine("Обновить файл?");
            if (Console.ReadLine().ToUpper() == "Y") ClearModel();
            else
            {
                using (var reader = new StreamReader(SetUp.Path))
                {
                    string CntOfRecList; // getting RecList
                    CntOfRecList = reader.ReadLine();
                    int StringCnt = Convert.ToInt16(CntOfRecList);
                    for(int i = 0; i < StringCnt; i++)
                    {
                        string line = reader.ReadLine();
                        string[] temp = line.Split(new[] { ' ' });
                        Model.RecList.Add(new List<int>());
                        for (int j = 2; j < Convert.ToInt16(temp[1]) + 2; j++)
                        {
                            Model.RecList[i].Add(Convert.ToInt16(temp[j]));
                        }
                    }

                    string templine = reader.ReadLine(); // getting QueList
                    string[] QueTemp = templine.Split(new[] { ' ' });
                    if (Convert.ToInt16(QueTemp[1]) != 0)
                    {
                        int index = 2;
                        for (int i = 0; i < Convert.ToInt16(QueTemp[1]); i++)
                        {
                            Model.QueList.Add(new Tuple<int, int>(Convert.ToInt16(QueTemp[index]), Convert.ToInt16(QueTemp[index+1])));
                            index+=2;
                        }
                    }
                    else Console.WriteLine("Очередь путса");

                    int CntOfRes;
                    string tempCntOfRes;
                    tempCntOfRes = reader.ReadLine();
                    CntOfRes = Convert.ToInt16(tempCntOfRes);
                    Model.vRes_s = new string[CntOfRes];
                    for (int i = 0; i < CntOfRes; i++)
                        Model.vRes_s[i] = reader.ReadLine();

                    Model.CntOfFreeRes = Convert.ToInt16(reader.ReadLine());
                    Model.CrtIndexInList = Convert.ToInt16(reader.ReadLine());
                }
            }
        }
        public static bool On()
        {
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory() + @"\Resmod00"))
                {
                    Console.WriteLine("Использовать существующий стандартный файл Resmod00?");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        Path = Directory.GetCurrentDirectory() + @"\Resmod00";
                        GetModel();
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("Создать стандартный файл?");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        Path = Directory.GetCurrentDirectory() + @"\Resmod00";
                        ClearModel();
                        return true;
                    }
                };
                Console.WriteLine("Введите полный адрес нестандартного файла:");
                Path = Console.ReadLine();
                if (File.Exists(Path))
                {
                    GetModel();
                    return true;
                }
                else
                {
                    ClearModel();
                    return true;
                }
            }
            catch (IOException) { Console.WriteLine("Файл не открылся."); return false; }
            //catch (Exception) { Console.WriteLine("Ошибка ввода-вывода."); return false; }
        }
    }




    static class Model
    {
        public static string[] vRes_s; //Модель набора ресурсов
        public static int CntOfFreeRes; // count of free res
        public static List<List<int>> RecList = new List<List<int>>(); // request list
        public static List<Tuple<int, int>> QueList = new List<Tuple<int, int>>(); // Queue list: 1st int - remaining time, 2nd int - count of res
        public static int CrtIndexInList = 0; // curent index in list

        public static void queue(int ResCnt)
        {
            Console.WriteLine("Введите приемлемое время ожидания (целое число - кол-во секунд)");
            int CntSec;
            while (true)
            {
                string UserInput;
                UserInput = Console.ReadLine();
                if (Int32.TryParse(UserInput, out CntSec))
                {
                    if (CntSec > 0)
                        break;
                }
                Console.WriteLine("Ошибка ввода, попробуйте ещё раз");
            }
            QueList.Add(new Tuple<int, int>(CntSec, ResCnt));
        }

        public static void Occupy(string cn)
        {
            if ((Convert.ToInt16(cn) > vRes_s.Length) | (Convert.ToInt16(cn) <= 0))
                throw new ResIdInvalid();

            if (Convert.ToInt16(cn) > CntOfFreeRes)
            {
                var excp = new ResAreBusy();
                excp.Data.Add("ResCnt", Convert.ToInt16(cn));
                throw excp; 
            }

            else // getting res
            {
                RecList.Insert(CrtIndexInList, new List<int>());
                int cnt = Convert.ToInt16(cn);
                for (int i = 0; i < vRes_s.Length && cnt > 0; i++)
                {
                    if (vRes_s[i] == "F")
                    {
                        vRes_s[i] = "B";
                        cnt--;
                        RecList[CrtIndexInList].Add(i);
                    }
                }
                CntOfFreeRes -= Convert.ToInt16(cn);
                Console.WriteLine("Ваш уникальный номер запроса - " + CrtIndexInList + " используйте его для удаления запроса");
                CrtIndexInList++;
            }
        }

        public static void Free(string cn)
        {
            if ((Convert.ToInt16(cn) >= RecList.Count) | (Convert.ToInt16(cn) < 0))
                throw new RecIdInvalid();
            if (RecList[Convert.ToInt16(cn)][0] >= 0)
            {
                for (int i = 0; i < RecList[Convert.ToInt16(cn)].Count; i++)
                {
                    if (vRes_s[RecList[Convert.ToInt16(cn)][i]] == "B")
                    {
                        vRes_s[RecList[Convert.ToInt16(cn)][i]] = "F";
                        RecList[Convert.ToInt16(cn)][i] = -1;
                    }
                    else Console.WriteLine("Error");
                }
                CntOfFreeRes += RecList[Convert.ToInt16(cn)].Count;
                Console.WriteLine("Запрос удалён.");
            }
            else Console.WriteLine("Вы пытаетесь удалить уже удалённый запрос");
        }

        public static string Request()
        {
            return Convert.ToString(CntOfFreeRes);
        }

    }
    class Program
    {
        static bool flg = false;
        static DateTime StartTime = DateTime.Now;
        static DateTime StopTime;
        static TimerCallback tm = new TimerCallback(timer_func);
        static System.Threading.Timer timer = new System.Threading.Timer(tm, null, Timeout.Infinite, Timeout.Infinite);

        static void Main(string[] args)
        {
            string Command = "Continue";
            while (!SetUp.On()) ;
            do
            {
                if (flg)
                    continue;

                using (StreamWriter writetext = new StreamWriter(SetUp.Path))
                {
                    string Reqstr = " ";
                    Reqstr += Model.RecList.Count;
                    writetext.WriteLine(Reqstr);
                    Reqstr = " ";
                    for (int k = 0; k < Model.RecList.Count; k++)
                    {
                        Reqstr += Model.RecList[k].Count + " ";
                        for (int z = 0; z < Model.RecList[k].Count; z++)
                            Reqstr += Model.RecList[k][z].ToString() + " ";
                        writetext.WriteLine(Reqstr);
                        Reqstr = " ";
                    }
                    


                    string Questr = " ";
                    Questr += Model.QueList.Count;
                    for(int i = 0; i < Model.QueList.Count; i++)
                    {
                        Questr += " " + Model.QueList[i].Item1.ToString() + " " + Model.QueList[i].Item2.ToString();
                    }
                    writetext.WriteLine(Questr);


                    writetext.WriteLine(Model.vRes_s.Length);
                    for (int j = 0; j < Model.vRes_s.Length; j++)
                    {
                        writetext.WriteLine(Model.vRes_s[j].ToString());
                    }
                    writetext.WriteLine(Model.CntOfFreeRes);
                    writetext.WriteLine(Model.CrtIndexInList);
                }

              

               


                Console.WriteLine("Введите команду:");
                Command = Console.ReadLine();
                Command = Command.ToUpper();
                try
                {
                    if (Command == "REQUEST") Console.WriteLine(Model.Request());
                    if (Command == "OCCUPY")
                    {
                        Console.WriteLine("Введите количество ресурсов:");
                        Model.Occupy(Console.ReadLine());
                        Console.WriteLine("Ресурсы стали занятыми.");
                    };
                    if (Command == "FREE")
                    {
                        Console.WriteLine("Введите номер запроса:");
                        Model.Free(Console.ReadLine());
                        if(Model.QueList.Count > 0)
                        {
                            if(Model.CntOfFreeRes >= Model.QueList[0].Item2)
                            {
                                Console.WriteLine("Заявка вышла из очереди. Ресурсы заняты.");
                                Model.Occupy(Convert.ToString(Model.QueList[0].Item2));
                                if (Model.QueList.Count == 1)
                                {
                                    Model.QueList.RemoveAt(0);
                                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                                }
                                else
                                {
                                    timer.Change(Model.QueList[1].Item1 * 1000, Model.QueList[1].Item1 * 1000);
                                    Model.QueList.RemoveAt(0);
                                }
                            }
                        }
                    };
                }
                catch (OverflowException) { Console.WriteLine("Такого ресурса нет."); }
                catch (FormatException) { Console.WriteLine("Такого ресурса нет."); }
                catch (ResIdInvalid) { Console.WriteLine("Вы пытаетесь занять больше ресуросов чем есть в системе или вы ввели отрицательное число."); }
                catch (RecIdInvalid) { Console.WriteLine("Такого запроса не существут или вы ввели отрицательное число"); }
                catch (ResWasFree) { Console.WriteLine("Ресурс был свободен."); }
                catch (ResAreBusy excp)
                {
                    Console.WriteLine("Все ресурсы заняты. Хотите встать в очередь? Y/N?");
                    string chs;
                    chs = Console.ReadLine();
                    int temp = (short)excp.Data["ResCnt"];
                    Console.WriteLine(temp);
                    if (chs.ToUpper() == "Y")
                        Model.queue(temp);
                    else Console.WriteLine("Запрос анулирован");
                }
                catch (ResIsBusy) { Console.WriteLine("ресурс уже занят."); }


                if (Model.QueList.Count == 0)
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                else
                {
                    timer.Change(Model.QueList[0].Item1 * 1000, Model.QueList[0].Item1 * 1000);
                    StartTime = DateTime.Now;
                }




                for (int i = 0; i < Model.vRes_s.Length; i++)
                    Console.Write(Model.vRes_s[i] + "   ");
                Console.WriteLine();
            }
            while (Command != "");
        }

        private static void ListTimeChange()
        {
            TimeSpan elapsedTime = StopTime.Subtract(StartTime);
            for (int i = 0; i < Model.QueList.Count; i++)
            {
                if (Model.QueList[i].Item1 - (elapsedTime.Seconds) < 0)
                    Model.QueList[i] = new Tuple<int, int>(0, Model.QueList[i].Item2);
                else
                    Model.QueList[i] = new Tuple<int, int>(Model.QueList[i].Item1 - (elapsedTime.Seconds), Model.QueList[i].Item2);
            }
        }

        static void timer_func(object obj)
        {
            flg = true;
            StopTime = DateTime.Now;
            SendKeys.SendWait("Время ожидания запроса на ресурс истекло!{ENTER}");
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            if (Model.CntOfFreeRes <= Model.QueList[0].Item2)
            {
                while (true)
                {
                    Console.WriteLine("Ресурсы все еще заняты. Ожидать его освобождения далее? Y/N?");
                    string answer = Console.ReadLine();
                    if (answer.ToUpper() == "Y")
                    {
                        int waitTime;
                        while (true)
                        {
                            Console.WriteLine("Введите время ожидания.");
                            try
                            {
                                waitTime = Convert.ToInt16(Console.ReadLine());
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("Неверный ввод, попробуйте снова.");
                            }
                        }
                        ListTimeChange();
                        Model.QueList[0] = new Tuple<int, int>(waitTime, Model.QueList[0].Item2);
                        Console.WriteLine("Время ожидания увеличено");
                        timer.Change(Model.QueList[0].Item1 * 1000, Model.QueList[0].Item1 * 1000);
                        break;
                    }
                    else if (answer.ToUpper() == "N")
                    {
                        Model.QueList.RemoveAt(0);
                        Console.WriteLine("Запрос успешно удален.");
                        ListTimeChange();
                        if (Model.QueList.Count > 0)
                            timer.Change(Model.QueList[0].Item1 * 1000, Model.QueList[0].Item1 * 1000);
                        else
                            timer.Change(Timeout.Infinite, Timeout.Infinite);
                        Console.WriteLine("Введите команду:");
                        break;
                    }
                    else
                        Console.WriteLine("Введите ответ снова.");
                }
            }
            else
            {
                Model.Occupy(Convert.ToString(Model.QueList[0].Item2));
                Console.WriteLine("Ресурсы теперь заняты.");
                Model.QueList.RemoveAt(0);
                ListTimeChange();
                if (Model.QueList.Count >= 1)
                    timer.Change(Model.QueList[0].Item1 * 1000, Model.QueList[0].Item1 * 1000);
                else
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            StartTime = DateTime.Now;
            flg = false;
        }

    }
}
