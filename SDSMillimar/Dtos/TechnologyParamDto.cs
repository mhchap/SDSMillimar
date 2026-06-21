using System.ComponentModel;

namespace SDSMillimar.Dtos
{
    public class TechnologyParamDto : INotifyPropertyChanged
    {
        public long Id { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public int MeasureType { get; set; }
        public int Sort { get; set; }

        private double compensationValue;

        public double CompensationValue
        {
            get { return compensationValue; }
            set { compensationValue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompensationValue))); }
        }

        private double filterValue;

        public double FilterValue
        {
            get { return filterValue; }
            set { filterValue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterValue))); }
        }


        private double targetValue;

        public double TargetValue
        {
            get { return targetValue; }
            set { targetValue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetValue))); }
        }

        private double upperTolerance;

        public double UpperTolerance
        {
            get { return upperTolerance; }
            set { upperTolerance = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpperTolerance))); }
        }
        private double lowerTolerance;

        public double LowerTolerance
        {
            get { return lowerTolerance; }
            set { lowerTolerance = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LowerTolerance))); }
        }


        private bool isDelete;

        public bool IsDelete
        {
            get { return isDelete; }
            set { isDelete = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDelete))); }
        }


        private string deviceIds;

        public string DeviceIds
        {
            get { return deviceIds; }
            set
            {
                deviceIds = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceIds)));
            }
        }


        private double measuredValue;

        public double MeasuredValue
        {
            get { return measuredValue; }
            set
            {
                measuredValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasuredValue)));
            }
        }

        private double cuValue;

        public double CUValue
        {
            get { return cuValue; }
            set
            {
                cuValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CUValue)));
            }
        }

        private double clValue;

        public double CLValue
        {
            get { return clValue; }
            set
            {
                clValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CLValue)));
            }
        }

        private double realValueA;

        public double RealValueA
        {
            get { return realValueA; }
            set { realValueA = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RealValueA))); }
        }

        private double realValueB;

        public double RealValueB
        {
            get { return realValueB; }
            set { realValueB = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RealValueB))); }
        }

        private bool? status;

        public bool? Status
        {
            get { return status; }
            set { status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
        }


        private bool _isMeasured;
        /// <summary>
        /// 是否已测量完成（控制UI阶段）
        /// </summary>
        public bool IsMeasured
        {
            get => _isMeasured;
            set
            {
                _isMeasured = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMeasured)));
            }
        }

        private double testValue = 0.0;

        public double TestValue
        {
            get { return testValue; }
            set
            {
                testValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestValue)));
            }
        }

        private int measureCount;

        public int MeasureCount
        {
            get { return measureCount; }
            set { measureCount = value; }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
