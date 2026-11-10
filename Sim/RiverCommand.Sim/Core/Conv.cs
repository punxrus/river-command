namespace RiverCommand.Sim
{
    /// <summary>Unit conversions at the content/UI boundary. The sim uses meters and seconds.</summary>
    public static class Conv
    {
        public const double MetersPerNauticalMile = 1852.0;
        public const double MetersPerFoot = 0.3048;
        public const double MpsPerKnot = MetersPerNauticalMile / 3600.0;

        public static double KnotsToMps(double knots) => knots * MpsPerKnot;
        public static double MpsToKnots(double mps) => mps / MpsPerKnot;
        public static double FeetToMeters(double feet) => feet * MetersPerFoot;
        public static double MetersToFeet(double meters) => meters / MetersPerFoot;
        public static double NmToMeters(double nm) => nm * MetersPerNauticalMile;
    }
}
