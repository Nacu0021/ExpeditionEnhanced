namespace ExpeditionEnhanced
{
    public abstract class CustomBurden : CustomContent
    {
        /// <summary>The % amount of how much the burden increases the expedition score.</summary>
        public abstract float ScoreMultiplier { get; }

        public CustomBurden() : base() { }
    }
}
