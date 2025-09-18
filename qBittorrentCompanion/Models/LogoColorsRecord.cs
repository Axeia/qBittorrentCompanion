namespace qBittorrentCompanion.Models
{
    public record LogoColorsRecord(
        string Q, 
        string B, 
        string C, 
        string GradientCenter, 
        string GradientFill, 
        string GradientRim
    )
    {
        /// <summary>
        /// Dark colors to have high contrast on light backgrounds
        /// </summary>
        public static LogoColorsRecord LightModeDefault => new(
            Q: "#000",
            B: "#000",
            C: "#000",
            GradientCenter: "maroon",
            GradientFill: "red",
            GradientRim: "maroon"
        );

        /// <summary>
        /// Light colors to have high contrast on dark backgrounds
        /// </summary>
        public static LogoColorsRecord DarkModeDefault => new(
            Q: "#fff",
            B: "#fff",
            C: "#fff",
            GradientCenter: "maroon",
            GradientFill: "red",
            GradientRim: "maroon"
        );
    }
}
