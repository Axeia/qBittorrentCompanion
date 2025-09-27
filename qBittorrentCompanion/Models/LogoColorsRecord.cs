using Avalonia.Media;

namespace qBittorrentCompanion.Models
{
    public record LogoColorsRecord(
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
        public static LogoColorsRecord LightModeDefault => new(
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
        public static LogoColorsRecord DarkModeDefault => new(
            Q: Colors.White,
            B: Colors.White,
            C: Colors.White,
            GradientCenter: Colors.Maroon,
            GradientFill: Colors.Red,
            GradientRim: Colors.Maroon
        );
    }
}
