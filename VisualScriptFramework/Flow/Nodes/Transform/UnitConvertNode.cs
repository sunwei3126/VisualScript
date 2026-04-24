using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Nodes.Transform
{
    /// <summary>
    /// Converts a numeric value between common IoT engineering units
    /// (e.g., Celsius ? Fahrenheit, m/s ? km/h, Pa ? hPa).
    /// </summary>
    public sealed class UnitConvertNode : LogicNode
    {
        public ValueInput  inputValue  { get; private set; }
        public ValueInput  fromUnit    { get; private set; }
        public ValueInput  toUnit      { get; private set; }
        public ValueOutput outputValue { get; private set; }

        protected override void Definition()
        {
            inputValue  = ValueInput<double>(nameof(inputValue), 0.0);
            fromUnit    = ValueInput<EngineeringUnit>(nameof(fromUnit),  EngineeringUnit.Celsius);
            toUnit      = ValueInput<EngineeringUnit>(nameof(toUnit),    EngineeringUnit.Fahrenheit);
            outputValue = ValueOutput<double>(nameof(outputValue), Convert);

            Requirement(inputValue, outputValue);
            Requirement(fromUnit,   outputValue);
            Requirement(toUnit,     outputValue);
        }

        private double Convert(Flow flow)
        {
            var v    = flow.GetValue<double>(inputValue);
            var from = flow.GetValue<EngineeringUnit>(fromUnit);
            var to   = flow.GetValue<EngineeringUnit>(toUnit);

            // First normalise to SI base unit, then convert to target
            var si = ToSI(v, from);
            return FromSI(si, to);
        }

        private static double ToSI(double v, EngineeringUnit u)
        {
            switch (u)
            {
                case EngineeringUnit.Celsius:    return v + 273.15;          // ¡ú Kelvin
                case EngineeringUnit.Fahrenheit: return (v - 32) * 5.0 / 9 + 273.15;
                case EngineeringUnit.Kelvin:     return v;
                case EngineeringUnit.Percent:    return v / 100.0;           // ¡ú fraction
                case EngineeringUnit.Fraction:   return v;
                case EngineeringUnit.MetersPerSecond: return v;              // base
                case EngineeringUnit.KilometersPerHour: return v / 3.6;
                case EngineeringUnit.MilesPerHour: return v * 0.44704;
                case EngineeringUnit.Pascal:     return v;                   // base
                case EngineeringUnit.HectoPascal: return v * 100.0;
                case EngineeringUnit.Millimeter: return v / 1000.0;          // ¡ú metres
                case EngineeringUnit.Centimeter: return v / 100.0;
                case EngineeringUnit.Meter:      return v;
                case EngineeringUnit.Kilometer:  return v * 1000.0;
                default: return v;
            }
        }

        private static double FromSI(double si, EngineeringUnit u)
        {
            switch (u)
            {
                case EngineeringUnit.Celsius:    return si - 273.15;
                case EngineeringUnit.Fahrenheit: return (si - 273.15) * 9.0 / 5 + 32;
                case EngineeringUnit.Kelvin:     return si;
                case EngineeringUnit.Percent:    return si * 100.0;
                case EngineeringUnit.Fraction:   return si;
                case EngineeringUnit.MetersPerSecond:   return si;
                case EngineeringUnit.KilometersPerHour: return si * 3.6;
                case EngineeringUnit.MilesPerHour:      return si / 0.44704;
                case EngineeringUnit.Pascal:      return si;
                case EngineeringUnit.HectoPascal: return si / 100.0;
                case EngineeringUnit.Millimeter:  return si * 1000.0;
                case EngineeringUnit.Centimeter:  return si * 100.0;
                case EngineeringUnit.Meter:       return si;
                case EngineeringUnit.Kilometer:   return si / 1000.0;
                default: return si;
            }
        }
    }

    public enum EngineeringUnit
    {
        // Temperature
        Celsius, Fahrenheit, Kelvin,
        // Ratio
        Percent, Fraction,
        // Speed
        MetersPerSecond, KilometersPerHour, MilesPerHour,
        // Pressure
        Pascal, HectoPascal,
        // Distance
        Millimeter, Centimeter, Meter, Kilometer
    }
}
