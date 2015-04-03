using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading;
using CustomCollections;

namespace CustomCollections
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomList<string> stringList = new CustomList<string>();

            #region Concurrently adding\removing elements
            {
                Thread th1 = new Thread(() => { ThreadsWork(stringList); }) { Name = "Thread #1" };
                th1.Start();
                Thread th2 = new Thread(() => { ThreadsWork(stringList); }) { Name = "Thread #22" };
                th2.Start();
                Thread th3 = new Thread(() => { ThreadsWork(stringList); }) { Name = "Thread #333" };
                th3.Start();

                th1.Join();
                th2.Join();
                th3.Join();
                th1.Abort();
                th2.Abort();
                th3.Abort();
            }
            #endregion

            Console.WriteLine();

            Console.WriteLine("Elements in collection: ");
            Print(stringList);

            stringList.Sort();
            Console.WriteLine();

            Console.WriteLine("Collection after sorting: ");
            Print(stringList);


            Console.WriteLine();
            Console.WriteLine(String.Format("Contains(\"7\"): {0}", stringList.Contains("7")));

            Console.WriteLine();
            Console.WriteLine(String.Format("Position of '7' in list: {0}", stringList.BinarySearch("7")));
            Console.WriteLine(String.Format("Search element '7' using Array class: {0}", Array.BinarySearch(stringList.ToArray(), 0, stringList.Count, "7")));
            Console.WriteLine();
            Console.WriteLine(String.Format("Position to insert '15' into list: {0}", ~stringList.BinarySearch("15")));

            Console.WriteLine();
            Console.WriteLine(String.Format("IndexOf(\"7\"): {0}", stringList.IndexOf("7")));

            Console.WriteLine();
            Console.WriteLine(String.Format("Remove(\"7\"): {0}", stringList.Remove("7")));

            Console.WriteLine();
            stringList.RemoveAt(7);
            Console.WriteLine(String.Format("RemoveAt(7)"));

            Console.WriteLine();
            stringList.Insert(7, "Inserted at 7");
            Console.WriteLine(String.Format("InsertAt(7)"));

            Console.WriteLine();
            Console.WriteLine("Elements after removing\\inserting: ");
            Print(stringList);

            Console.WriteLine();
            Console.WriteLine(String.Format("Capacity before trim: {0}", stringList.Capacity));
            stringList.TrimExcess();
            Console.WriteLine(String.Format("Capacity after trim: {0}", stringList.Capacity));

            Console.WriteLine();
            stringList.Clear();
            Console.WriteLine(String.Format("Capacity after clear: {0} ; Count after clear: {1}", stringList.Capacity, stringList.Count));
            stringList.TrimExcess();
            Console.WriteLine(String.Format("Capacity after trim: {0}", stringList.Capacity));

            Console.ReadLine();

        }

        static void ThreadsWork(CustomList<string> stringList)
        {

            for (int i = 0; i < 10; i++)
            {
                stringList.Synchronized.Add(i.ToString());
                Console.WriteLine("Thread: {0} added string '{1}'", Thread.CurrentThread.Name, i.ToString());
            }

            Random random = new Random();

            for (int i = 0; i < 5; i++)
            {
                int place = random.Next(15);
                if (place < stringList.Synchronized.Count)
                {
                    stringList.Synchronized.RemoveAt(place);
                    Console.WriteLine("Thread: {0} removed string at place '{1}'", Thread.CurrentThread.Name, place.ToString());
                }
                else
                {
                    stringList.Synchronized.RemoveAt(i);
                    Console.WriteLine("Thread: {0} removed string at place '{1}'", Thread.CurrentThread.Name, i.ToString());

                }
            }

            Console.WriteLine(String.Format("{0} end work. Elements count: {1}", Thread.CurrentThread.Name, stringList.Synchronized.Count));
        }

        static void Print<T>(IList<T> list)
        {
            {
                int i = 0;
                foreach (T item in list)
                {
                    Console.WriteLine(String.Format("list[{0}] = {1}", i, item));
                    i++;
                }
            }
        }
    }
}
