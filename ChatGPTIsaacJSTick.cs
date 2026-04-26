#region Using declarations
using System;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
    public class IsaacJSTick : BarsType
    {
        private int tradeCount;
        private double currentVolume;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "IsaacJSTick (True Jigsaw Logic)";
                BarsPeriod = new BarsPeriod { BarsPeriodType = (BarsPeriodType)100, Value = 100 };
                BuiltFrom = BarsPeriodType.Tick;
                DefaultChartStyle = Gui.Chart.ChartStyleType.CandleStick;
                DaysToLoad = 3;
                IsIntraday = true;
            }
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
            // 🔴 Ignore non-trade updates (best approximation)
            if (volume <= 0)
                return;

            // 🔴 First bar
            if (bars.Count == 0)
            {
                AddBar(bars, close, close, close, close, time, volume);
                tradeCount = 1;
                currentVolume = volume;
                return;
            }

            // 🔴 If bar is complete → start new one
            if (tradeCount == bars.BarsPeriod.Value)
            {
                AddBar(bars, close, close, close, close, time, volume);

                tradeCount = 1;
                currentVolume = volume;
                return;
            }

            // 🔴 Update current bar
            double barHigh = Math.Max(bars.GetHigh(bars.Count - 1), close);
            double barLow = Math.Min(bars.GetLow(bars.Count - 1), close);

            currentVolume += volume;

            UpdateBar(
                bars,
                barHigh,
                barLow,
                close,
                time,
                currentVolume
            );

            tradeCount++;
        }

        public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
        {
            return 3;
        }

        public override double GetPercentComplete(Bars bars, DateTime now)
        {
            if (bars.BarsPeriod.Value == 0)
                return 0;

            return (double)tradeCount / bars.BarsPeriod.Value;
        }

        public override string ChartLabel(DateTime time)
        {
            return Name;
        }
    }
}
