using Avalonia.Controls.Shapes;
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
            Q: Color.Parse("#7FD8C9"),
            B: Color.Parse("#7FD8C9"),
            C: Color.Parse("#7FD8C9"),
            GradientCenter: Color.Parse("#4AB8B0"),
            GradientFill: Color.Parse("#1F7A78"),
            GradientRim: Color.Parse("#FFFFFF")
        );


        /// <summary>
        /// Light colors to have high contrast on dark backgrounds
        /// </summary>
        public static LogoDataRecord DarkModeDefault => new(
            Q: Color.Parse("#7FD8C9"),
            B: Color.Parse("#7FD8C9"),
            C: Color.Parse("#7FD8C9"),
            GradientCenter: Color.Parse("#4AB8B0"),
            GradientFill: Color.Parse("#1F7A78"),
            GradientRim: Color.Parse("#FFFFFF")
        );
    }
}