using System;
using System.Collections.Generic;
using System.Linq;
using GAF;
using GAF.Extensions;
using GAF.Operators;
using Math = System.Math;

namespace TravellingSalesman
{
    internal class Program
    {

        static Random r = new Random();
        private static int NoP = 4;//Number of processors
        private static int NoJ = 5;//Number of jobs
        private static List<List<int>> TL;// list of times
        private static List<(int first, int second)> Dp = new List<(int first, int second)>();// Dependance

        public static void Generate(int nj)
        {
            TL = new List<List<int>>();
         
            for (int i = 0; i < NoP; ++i)
            {
                TL.Add(new List<int>());
               
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
        static double CalculateFitness(Chromosome h)
        {
            
            int time = TimeCheck(h);
            if (time == int.MaxValue) return 0;
            //Console.WriteLine(1 - (double)time / 1000);
            return 1 - (double)time / 1000;
        }

        static private int TimeCheck(Chromosome h)
        {
           
            List<List<int>> IP;
            IP = new List<int>[NoP].ToList();
            for(int i = 0; i < IP.Count; ++i)
            {
                IP[i] = new List<int>();
            }

            for(int i = 0; i < h.Genes.Count; ++i)
            {
                IP[i * NoP / NoJ].Add((int)h.Genes[i].ObjectValue);
            }




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
                    // debbug(IP);
                    if (IP[i].Count == 0) continue;

                    foreach (var Job in IP[i])
                    {

                        if (JL[Job] == false && CheckDependancies(Job, JL))
                        {
                            anychanges = true;
                            PL[i] = Math.Max(PL[i], MaxValOFDependancies(Job, EL));//dodanie czekania na nieskonczone procesy
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
        static private int MaxValOFDependancies(int Job, List<int> Times)
        {
            int max = 0;
            foreach (var item in Dp)
                if (item.second == Job)
                    if (Times[Job] > max)
                        max = Times[Job];


            return max;
        }
        static private bool CheckDependancies(int Job, List<bool> Jobs)
        {
            foreach (var item in Dp)
                if (item.second == Job)
                    if (Jobs[item.first] == false)
                        return false;
            return true;
        }



        private static void Main(string[] args)
        {
            const int populationSize = 100;

        
            var population = new Population();
            Generate(100);
            //create the chromosomes
            for (var p = 0; p < populationSize; p++)
            {

                var chromosome = new Chromosome();
                for(int i = 0; i < NoJ; ++i)
                {
                    chromosome.Genes.Add(new Gene(i));
                }


                var rnd = GAF.Threading.RandomProvider.GetThreadRandom();
                chromosome.Genes.ShuffleFast(rnd);

                population.Solutions.Add(chromosome);
            }
            //create the memory
            var mem = new Memory(2,1);
            //create the elite operator
            var elite = new Elite(5);
            //create the crossover operator
            var crossover = new Crossover(0.8)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };
            //create the mutation operator
            var mutate = new SwapMutate(0.2);
            //create the GA
            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //hook up to some useful events
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            //add the operators
            ga.Operators.Add(mem);
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            //run the GA
            ga.Run(Terminate);
            Console.Read();
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            List<List<int>> IP;
            IP = new List<int>[NoP].ToList();
            for (int i = 0; i < IP.Count; ++i)
            {
                IP[i] = new List<int>();
            }

            for (int i = 0; i < fittest.Genes.Count; ++i)
            {
                IP[i * NoP / NoJ].Add((int)fittest.Genes[i].ObjectValue);
            }

            Print(IP, fittest);

        }
        static public void Print(List<List<int >> BO,Chromosome h)
        {
            Console.WriteLine("Input: \nNumber of processors: " + NoP
                + "\nNumber of jobs: " + NoJ +
                "\nArray of jobs completion times for each processor:\n");
            for (int i = 0; i < NoP; ++i)
            {
                Console.Write("processor " + i + ".    ");
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
            if (TimeCheck(h) == int.MaxValue)
                Console.WriteLine("Time: Not found");
            else
                Console.WriteLine("Time: " + TimeCheck(h));
            Console.WriteLine("Proc.   Jobs");
            for (int i = 0; i < NoP; ++i)
            {
                Console.Write(i + ".    |");
                foreach (var job in BO[i])
                    Console.Write(job + " ");
                Console.Write("\n");
            }
        }

        

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            var distanceToTravel = TimeCheck(fittest);
            Console.WriteLine("Generation: {0}, Fitness: {1}, Time: {2}", e.Generation, fittest.Fitness, distanceToTravel);

        }

        
        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 400;
        }

    }
}















