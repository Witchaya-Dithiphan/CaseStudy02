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

      // ตัวแปร Object สำหรับทำ Lock / Condition Variable
      static readonly object lockObj = new object();
      // Flag ระบุว่า Producer ทำงานเสร็จสิ้นแล้วหรือไม่
      static bool producersFinished = false;

      static void EnQueue(int eq)
      {
         lock (lockObj)
         {
               // หาก Queue เต็ม (Count == 10) ให้รอ
               while (Count == TSBuffer.Length)
               {
                  Console.WriteLine("............[Thread-{0}]:Queue full, waiting.........", Thread.CurrentThread.Name);
                  Monitor.Wait(lockObj);
               }

               TSBuffer[Back] = eq;
               Back = (Back + 1) % TSBuffer.Length;
               Count += 1;

               // ส่งสัญญาณบอก Consumer ว่ามีข้อมูลเข้ามาใน Queue แล้ว
               Monitor.PulseAll(lockObj);
         }
      }

      static int DeQueue()
      {
         lock (lockObj)
         {
               // หาก Queue วาง (Count == 0) และ Producer ยังผลิตไม่เสร็จ ให้รอ
               while (Count == 0 && !producersFinished)
               {
                  Monitor.Wait(lockObj);
               }

               // หาก Queue วาง และ Producer ผลิตเสร็จสิ้นแล้ว ให้คืนค่า -1 เป็นสัญญาณจบ
               if (Count == 0 && producersFinished)
               {
                  return -1;
               }

               int x = TSBuffer[Front];
               Front = (Front + 1) % TSBuffer.Length;
               Count -= 1;

               // ส่งสัญญาณบอก Producer ว่า Queue มีที่ว่างแล้ว
               Monitor.PulseAll(lockObj);
               return x;
         }
      }

      static void th01(object t)
      {
         Thread.CurrentThread.Name = t.ToString();
         int i;

         for (i = 1; i < 51; i++)
         {
               EnQueue(i);
               Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
         }
      }

      static void th011(object t)
      {
         Thread.CurrentThread.Name = t.ToString();
         int i;

         for (i = 100; i < 151; i++)
         {
               EnQueue(i);
               Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
         }
      }

      static void th02(object t)
      {
         Thread.CurrentThread.Name = t.ToString();
         int i;
         int j;

         for (i = 0; i < 60; i++)
         {
               j = DeQueue();
               if (j == -1) break; // จบการทำงานเมื่อไม่มีข้อมูลเหลือและ Producer ผลิตเสร็จแล้ว

               Console.WriteLine("j={0}, thread:{1}", j, t);
               Thread.Sleep(16); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
         }
         Console.WriteLine("thread-{0} exit", t);
      }

      static void Main(string[] args)
      {
         Console.WriteLine("Press any key to start...");
         Console.ReadKey();

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

         // รอให้ Producer ทั้งสองทำหน้าที่ใส่ข้อมูลเข้า Queue จนครบ
         t1.Join();
         t11.Join();

         // ส่งสัญญาณแจ้ง Consumer ทุกตัวว่า Producer ทำงานเสร็จสิ้นแล้ว
         lock (lockObj)
         {
               producersFinished = true;
               Monitor.PulseAll(lockObj);
         }

         Console.WriteLine("Press any key to exit...");
         Console.ReadKey();
      }
   }
}