using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public static class DataRecords
    {
        public static DataRecord GetDataByTemperature(double Temperature, IEnumerable<Specie> Specie)
        {
            int recordCount = Specie.First().DataRecords.Count;
            for (int i = 0; i < recordCount; i++)
            {
                List<double> interval = Specie.First().DataRecords.ElementAt(i).TemperatureRange;
                double m_min = interval.Min();
                double m_max = interval.Max();

                if (Temperature >= m_min && Temperature <= m_max)
                {
                    return Specie.First().DataRecords.ElementAt(i);
                }
            }
            return Specie.First().DataRecords.ElementAt(0);
        }
    }
}
