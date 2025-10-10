using Avalonia.Media;

namespace qBittorrentCompanion.Models
{
    public record LogoDataRecord(
        Color Q,
        Color B,
        Color C,
        Color GradientCenter,
        Color GradientFill,
        Color GradientRim
    )
    {
        /// <summary>
        /// Dark colors to have high contrast on light backgrounds
        /// </summary>
        public static LogoDataRecord LightModeDefault => new(
            Q: Colors.Black,
            B: Colors.Black,
            C: Colors.Black,
            GradientCenter: Colors.Maroon,
            GradientFill: Colors.Red,
            GradientRim: Colors.Maroon
        );

        /// <summary>
        /// Light colors to have high contrast on dark backgrounds
        /// </summary>
        public static LogoDataRecord DarkModeDefault => new(
            Q: Colors.White,
            B: Colors.White,
            C: Colors.White,
            GradientCenter: Colors.Maroon,
            GradientFill: Colors.Red,
            GradientRim: Colors.Maroon
        );
    }
}
