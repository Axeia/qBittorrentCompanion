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
        public static LogoColorsRecord LightDefault => new(
            Q: "#fff",
            B: "#fff",
            C: "#fff",
            GradientCenter: "maroon",
            GradientFill: "red",
            GradientRim: "maroon"
        );

        public static LogoColorsRecord DarkDefault => new(
            Q: "#000",
            B: "#000",
            C: "#000",
            GradientCenter: "maroon",
            GradientFill: "red",
            GradientRim: "maroon"
        );
    }
}
