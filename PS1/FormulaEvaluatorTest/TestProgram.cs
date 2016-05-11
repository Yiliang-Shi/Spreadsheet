using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormulaEvaluator;

namespace FormulaEvaluatorTest
{
    /// <summary>
    /// Tests FormularEvaluator Library
    /// </summary>
    class TestProgram
    {
        static Dictionary<string, int> valueTable = new Dictionary<string, int>();

        static void Main(string[] args)
        {
           
            List<string> input = new List<string>();
            List<int> result = new List<int>();
            LoadTest1(input, result);
            Evaluator.Lookup func = search;
            valueTable.Add("abc123", 10);
            valueTable.Add("b123", 20);
            valueTable.Add("abc1", 30);

            for (int i = 0;i < input.Count(); i++)
            {
                if (Evaluator.Evaluate(input[i], func) != result[i])
                    Console.WriteLine(Evaluator.Evaluate(input[i], func) + "is not equal " + result[i]);
            }


            Console.Read();
        }

        public static int search(string var)
        {
            int ans;
            if(valueTable.TryGetValue(var, out ans))
                return ans;
            throw new ArgumentException();
        }

        public static void LoadTest1(List<string> input,List<int> result)
        {
            double answer = 0;
            string exp = "0";
            input.Add(exp);
            result.Add((int)answer);

            answer = 0 + 1;
            exp = "0+1";
            input.Add(exp);
            result.Add((int)answer);

            answer = 0 + 1 - 2 + 5;
            exp = " 0 + 1-2+5";
            input.Add(exp);
            result.Add((int)answer);

            answer = 0 - 1 - 10 - 3 + 32;
            exp = "0 - 1-10-3+32";
            input.Add(exp);
            result.Add((int)answer);

            answer = 0 - 1 - 10 * (3 + 32);
            exp = "0 - 1-10*(3+32)";
            input.Add(exp);
            result.Add((int)answer);

            answer = 25 / 5 - 10 - 3 + 32;
            exp = "25/5 -10-3+32";
            input.Add(exp);
            result.Add((int)answer);

            answer = 15/(3+2-4*2+(6*3-10/(23-4)*5)-((4+4)*2));
            Console.WriteLine(answer);
            exp = "15/(3+2-4*2+(6*3-10/(23-4)*5)-((4+4)*2))";
            input.Add(exp);
            result.Add((int)answer);

            answer = 10;
            exp = "abc123";
            input.Add(exp);
            result.Add((int)answer);

            answer = 30;
            exp = "b123+abc123";
            input.Add(exp);
            result.Add((int)answer);

            answer = 170;
            exp = "abc123* b123 -abc1";
            input.Add(exp);
            result.Add((int)answer);

            answer = 15/(3+2-4*2+(6*3-10/(23-4)*5)-((4+4)*2));
            exp = "15/(3+2-4*2+(6*3-abc123/(23-4)*5)-((4+4)*2))";
            input.Add(exp);
            result.Add((int)answer);

            answer = 20 - 14 * (12 + 4 / 2 - 1) - (22 - 3 * 30);
            exp = "b123 - 14*(12+   4/ 0 - 1)-(22-3 * abc1)";
            input.Add(exp);
            result.Add((int)answer);
        }
       
    }
}
