#region Using declarations
using System;
using System.ComponentModel;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
    public class IsaacJSTick : BarsType
    {
        private int tickCounter = 0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description     = @"Jigsaw-style tick bars (counts each trade update)";
                Name            = "IsaacJSTick";
                BarsPeriod      = new BarsPeriod { BarsPeriodType = (BarsPeriodType)100, Value = 25 };
                BuiltFrom       = BarsPeriodType.Tick;
                DaysToLoad      = 3;
                IsIntraday      = true;
                IsTimeBased     = false;
            }
        }

        public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
        {
            period.Value = 25;
        }

        public override void ApplyDefaultValue(BarsPeriod period)
        {
            period.Value = 25;
        }

        public override string ChartLabel(DateTime time)
        {
            return time.ToString("HH:mm:ss");
        }

        public override double GetPercentComplete(Bars bars, DateTime time)
        {
            if (bars.BarsPeriod.Value == 0)
                return 0;

            return (double)tickCounter / bars.BarsPeriod.Value;
        }

        public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
        {
            return 3;
        }

        protected override void OnDataPoint(
            Bars bars,
            double open,
            double high,
            double low,
            double close,
            DateTime time,
            long volume,
            bool isBar,
            double bid,
            double ask)
        {
            // ✅ Filter: only count actual trades
            if (volume <= 0)
                return;

            // First bar
            if (bars.Count == 0)
            {
                AddBar(bars, open, high, low, close, time, volume);
                tickCounter = 1;
                return;
            }

            // Update current bar
            UpdateBar(bars, high, low, close, time, volume);

            // Count every trade update (Jigsaw-style)
            tickCounter++;

            // When threshold reached → close bar and start new one
            if (tickCounter >= bars.BarsPeriod.Value)
            {
                tickCounter = 0;

                AddBar(bars, close, close, close, close, time, 0);
            }
        }

        public override string ToString()
        {
            return Name + " " + BarsPeriod.Value;
        }
    }
}