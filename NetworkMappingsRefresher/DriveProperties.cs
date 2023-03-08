namespace NetEti.FileTools
{
    /// <summary>
    /// Enthält Informationen zu einem (Netzwerk-)Laufwerk.
    /// </summary>
    /// <remarks>
    /// 28.02.2023 Erik Nagel: created.
    /// </remarks>
    public class DriveProperties
    {
        /// <summary>
        /// Der Laufwerksbuchstabe.
        /// </summary>
        public string DriveLetter { get; set; }

        /// <summary>
        /// Der Laufwerkspfad.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Laufwerksstatus
        /// True: Laufwerk ist connected.
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Konstruktor - übernimmt driveLetter, path und isReady.
        /// </summary>
        /// <param name="driveLetter">Der Laufwerksbuchstabe.</param>
        /// <param name="path">Der Laufwerkspfad.</param>
        /// <param name="isReady">Laufwerksstatus - True: Laufwerk ist connected.</param>
        public DriveProperties(string driveLetter, string path, bool isReady)
        {
            DriveLetter = driveLetter;
            Path = path;
            IsReady = isReady;
        }

        /// <summary>
        /// Überschriebene ToString-Methode.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format($@"{this.DriveLetter}: {this.Path}, {this.IsReady.ToString()}");
        }
    }
}
    