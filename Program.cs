using System;
using System.Threading;

namespace OS_Problem_02
{
    class Thread_safe_buffer
    {
        static int[] TSBuffer = new int[10];
        static int Front = 0;
        static int Back = 0;
        static int Count = 0;
        static readonly object lockObj = new object();
        static int Thread_Putting_data = 2; //จำนวน producer ที่ยังทำงานอยู่
        const int No_Data = int.MinValue; //ค่าจริงๆ -2147483648 ใช้ค่าอื่นก็ได้ไม่ต้อง int.MinValue แต่วุ่นวาย

        static object[] ExitedThreads = new object[3]; //array เก็บ thread ที่จบงาน
        static int ExitedCount = 0;

        static void EnQueue(int eq, object t)
        {
            lock (lockObj)
            {
                while (Count == 10) //กัน buffer เต็ม
                {
                    Console.WriteLine("..........[Thread-{0}]:Queue full, waiting..........", t);
                    Monitor.Wait(lockObj);
                }

                TSBuffer[Back] = eq;
                Back++;
                Back %= 10;
                Count += 1;

                Monitor.PulseAll(lockObj); //ปลุก Thread อื่น
            }
        }

        static int DeQueue(object t)
        {
            lock (lockObj)
            {
                while (Count == 0 && Thread_Putting_data > 0) //buffer ว่าง -> ส่ง lock ให้ producer เอาข้อมูลเข้า queue
                {
                    Monitor.Wait(lockObj);
                }

                if (Count == 0 && Thread_Putting_data == 0) //buffer ว่างจริง
                {
                    return No_Data;
                }

                int x = TSBuffer[Front];
                Front++;
                Front %= 10;
                Count -= 1;

                Console.WriteLine("j={0}, thread:{1}", x, t);
                Monitor.PulseAll(lockObj);
                return x;
            }
        }

        static void th01(object t)
        {
            int i;

            for (i = 1; i < 51; i++)
            {
                EnQueue(i,t);
                Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }

            lock (lockObj)
            {
                Thread_Putting_data--; //producer ทำงานเสร็จแล้ว
                Monitor.PulseAll(lockObj); //ปลุก consumer ที่อาจรออยู่ (เผื่อไม่มีของเหลือ)
            }
        }

        static void th011(object t)
        {
            int i;

            for (i = 100; i < 151; i++)
            {
                EnQueue(i,t);
                Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }

            lock (lockObj)
            {
                Thread_Putting_data--;
                Monitor.PulseAll(lockObj);
            }
        }


        static void th02(object t)
        {
            int i;
            int j;

            for (i = 0; i < 60; i++)
            {
                j = DeQueue(t);
                if (j == No_Data) break; //ใน queue ไม่มีข้อมูลแล้ว (กรณี EnQueue เสร็จแล้ว เพราะมันเสร่อดึงเกิน) -> ออก loop
                
                Thread.Sleep(16); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }

            lock (lockObj) //thread จบงานจริงตรงนี้
            {
                ExitedThreads[ExitedCount] = t;
                ExitedCount++;
            }
        }
        static void Main(string[] args)
        {
            Thread t1 = new Thread(th01);
            Thread t11 = new Thread(th011);
            Thread t2 = new Thread(th02);
            Thread t21 = new Thread(th02);
            Thread t22 = new Thread(th02);

            t1.Start(100);
            t11.Start(200);
            t2.Start(1);
            t21.Start(2);
            t22.Start(3);

            t1.Join();
            t11.Join();
            t2.Join();
            t21.Join();
            t22.Join();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            for (int k = 0; k < ExitedCount; k++)
            {
                Console.WriteLine("thread-{0} exit", ExitedThreads[k]);
            }
        }
    }
}