namespace RemoteHealthcare.Bike
{
    interface IBike
    {
        public void Start(string bikeId = null);
        public void Stop();

        /// <summary>
        /// Sets the resistance of the bike. This value will influence the speed.
        /// </summary>
        /// <param name="resistance">The byte value used to set the resistance. The value gets
        /// converted to a percentage.</param>
        public void SetResistance(byte resistance);

        /// <summary>
        /// This method should be called when a new batch of data is available.
        /// Inside this method a call should be made to <see cref="DeviceManager.HandleData((DataTypes, float))"./>
        /// </summary>
        /// <param name="data">Is a Tuple containing a 
        /// <see cref="DataTypes"/> indicating the type of received data, and a
        /// float containing the value of the data.</param>
        public void DataReceived((DataTypes, float) data);
    }
}
