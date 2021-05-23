using System;
using System.Collections.Generic;
using System.Linq;

namespace AIP1
{
    class SchedulingProblem
    {
        static Random r = new Random();

        private static int NoP = 4;//Number of processors
        private static int NoJ = 5;//Number of jobs

        private int iter = 1000000;
        private List<List<int>> TL;// list of times
        private List<List<int>> WP;//Workplace
        private List<List<int>> BO;//Best output
        private List<(int first, int second)> Dp = new List<(int first, int second)>();// Dependance
        private const float StartHeat = 100;
        private float Heat = StartHeat;


        public SchedulingProblem()
        {
            TL = new List<List<int>>();
            WP = new List<List<int>>();
            BO = new List<List<int>>();
            for (int i = 0; i < NoP; ++i)
            {
                TL.Add(new List<int>());
                WP.Add(new List<int>());
                BO.Add(new List<int>());
            }

            int[,] r1 = {
                { 1, 2, 6, 2, 4 },
                { 3, 4, 9, 6, 9 },
                { 5, 1, 1, 7, 8 },
                { 7, 1, 5, 3, 6 }
            };
            Dp.Add((3, 2));
            Dp.Add((0, 4));
            for (int i = 0; i < NoP; ++i)
                for (int j = 0; j < NoJ; ++j)
                    TL[i].Add(r1[i, j]);
            for (int i = 0; i < NoJ; ++i)
            {
                WP[r.Next(0, NoP)].Add(i);
            }
            BO = Copy(WP);
        }

        public SchedulingProblem(int nj)
        {
            TL = new List<List<int>>();
            WP = new List<List<int>>();
            BO = new List<List<int>>();
            for (int i = 0; i < NoP; ++i)
            {
                TL.Add(new List<int>());
                WP.Add(new List<int>());
                BO.Add(new List<int>());
            }
            NoJ = nj;
            for (int i = 0; i < NoP; ++i)
                for (int j = 0; j < NoJ; ++j)
                    TL[i].Add(r.Next(5, 10));
            for (int i = 0, k = (int)Math.Sqrt(NoJ); i < k; ++i)
            {
                int random = (r.Next(0, NoJ));
                Dp.Add((random, r.Next(random + 1, NoJ)));
            }

            for (int i = 0; i < NoJ; ++i)
            {
                WP[r.Next(0, NoP)].Add(i);
            }
            BO = Copy(WP);
        }

        private int TimeCheck(List<List<int>> IP)
        {
            List<bool> JL = new List<bool>();//Joblist
            List<int> PL = new List<int>();//Processor list
            List<int> EL = new List<int>();//End time list
            for (int i = 0; i < NoJ; ++i)
                JL.Add(false);
            for (int i = 0; i < NoP; ++i)
                PL.Add(0);
            for (int i = 0; i < NoJ; ++i)
                EL.Add(0);

            bool anychanges = false;
            while (JL.Contains(false))
            {
                for (int i = 0; i < NoP; ++i)
                {
                    if (IP[i].Count == 0) continue;

                    foreach (var Job in IP[i])
                    {

                        if (JL[Job] == false && CheckDependancies(Job, JL))
                        {
                            anychanges = true;
                            PL[i] = Math.Max(PL[i], MaxValOFDependancies(Job, EL));//Waitong fo infinite process's
                            PL[i] += TL[i][Job];
                            EL[Job] = PL[i];
                            JL[Job] = true;
                        }
                        else
                            break;
                    }
                }
                if (anychanges == false)
                    return int.MaxValue;
                anychanges = false;
            }
            return EL.Max();
        }
        private int MaxValOFDependancies(int Job, List<int> Times)
        {
            int max = 0;
            foreach (var item in Dp)
                if (item.second == Job)
                    if (Times[Job] > max)
                        max = Times[Job];


            return max;
        }
        private bool CheckDependancies(int Job, List<bool> Jobs)
        {
            foreach (var item in Dp)
                if (item.second == Job)
                    if (Jobs[item.first] == false)
                        return false;
            return true;
        }

        private float AC(float v1, float v2)
        {
            return MathF.Exp((v1 - v2) / Heat);
        }

        public void Start()
        {
            List<List<int>> CWP;// Current Workplace
            CWP = Copy(WP);

            {
                int i;
                for ( i = 0; i < 100000 || TimeCheck(CWP) == int.MaxValue; ++i)
                    Reshuffle(CWP);//Search for starting possible solution

                Console.WriteLine("Starting solution found after "+i+" iterations");
            }
            int BOV, WPV, CWPV;
            BOV = WPV = CWPV = TimeCheck(CWP);

            for (int i = 0; i < iter; ++i)
            {
                //CWP =new List<List<int>>{ new List<int>{ 1 }, new List<int> { 2 }, new List<int> { 3 }, new List<int> { 4, 0 } };
                if (CWPV < WPV)
                {
                    //WP = Copy(CWP);
                    WPV = CWPV;
                    if (WPV < BOV)
                    {
                        Console.WriteLine("iteration " + i + " found new best =" + BOV);
                        BO = Copy(CWP);
                        BOV = WPV;
                    }
                }
                else
                {
                    if (r.NextDouble() < AC(WPV, CWPV))
                    {
                        //WP = Copy(CWP);
                        WPV = CWPV;
                    }
                }
                //Console.WriteLine(TimeCheck(BO));
                CoolDown();
                Reshuffle(CWP);
                CWPV = TimeCheck(CWP);
            }
        }

        private List<List<int>> Reshuffle(List<List<int>> OG)
        {
            int Processor, Job, Worker;
            for (int i = 0; i < Heat; ++i)
            {
                Processor = r.Next(0, NoP);
                if (OG[Processor].Count == 0) continue;
                Job = r.Next(0, OG[Processor].Count);
                Worker = OG[Processor][Job];
                OG[Processor].RemoveAt(Job);
                Processor = r.Next(0, NoP);
                Job = r.Next(0, OG[Processor].Count);
                OG[Processor].Insert(Job, Worker);
            }
            return OG;
        }


        private void CoolDown()
        {
            Heat += (-1 * StartHeat) / iter;
        }


        private List<List<int>> Copy(List<List<int>> OG)
        {
            List<List<int>> OT = new List<List<int>>();//Output
            for (int i = 0; i < NoP; ++i)
            {
                OT.Add(new List<int>());
            }
            for (int i = 0; i < OG.Count; ++i)
            {
                foreach (var job in OG[i])
                {
                    OT[i].Add(job);
                }
            }
            return OT;
        }

        public void Print()
        {
            Console.WriteLine("Input: \nNumber of processors: " + NoP
                + "\nNumber of jobs: " + NoJ +
                "\nArray of jobs completion times for each processor:\n" +
                " Proc.     job     ");
            for (int i = 0; i < NoP; ++i)
            {
                Console.Write(i + ".    ");
                foreach (var time in TL[i])
                {
                    if (time > 9)
                        Console.Write(time + " | ");
                    else
                        Console.Write(time + "  | ");
                }
                Console.Write("\n");
            }
            Console.WriteLine("Dependencies (first job must be done before the second)");
            for (int i = 0; i < Dp.Count(); ++i)
                Console.WriteLine(Dp[i].first + " " + Dp[i].second);
            Console.WriteLine("---------------------------------- \nOutput: ");
            if (TimeCheck(BO) == int.MaxValue)
                Console.WriteLine("Time: Not found");
            else
                Console.WriteLine("Time: " + TimeCheck(BO));
            Console.WriteLine("Proc.   Jobs");
            for (int i = 0; i < NoP; ++i)
            {
                Console.Write(i + ".    |");
                foreach (var job in BO[i])
                    Console.Write(job + " ");
                Console.Write("\n");
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            //SchedulingProblem W = new SchedulingProblem();//Default data to process. works for 4 processors and 5 jobs
            SchedulingProblem W = new SchedulingProblem(100);//Random data to process, Number of jobs to generate as arg
            W.Start();
            W.Print();
        }
    }
}
