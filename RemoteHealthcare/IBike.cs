namespace RemoteHealthcare
{
    public interface IBike
    {
        public void SetResistance(byte resistance);
        public void SetAirResistance(byte airResistanceCoefficient, byte windspeed, byte draftingFactor);
    }
}
