using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LAB33
{
    internal class NumbersGenerator
    {
        public List<int> Generate(int numberOfElements, int rangeFrom, int rangeTo, BackgroundWorker worker)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            var result = new List<int>(numberOfElements);
            for (int i = 0; i<numberOfElements; i++)
            {
                Task.Delay(random.Next(500, 1500)).Wait();
                result.Add(random.Next(rangeFrom, rangeTo));
                worker.ReportProgress((int)(((float)(i + 1) / ((float)numberOfElements) * 100)));
            }
            return result;
        }
    }
}
