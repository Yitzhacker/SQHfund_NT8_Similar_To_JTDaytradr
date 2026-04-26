#region Using declarations
using System;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
    public class ChatGPTIsaacJSTick : BarsType
    {
        private int tradeCount;
        private long currentVolume;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "ChatGPTIsaacJSTick (True Jigsaw Logic)";
                BarsPeriod = new BarsPeriod
                {
                    BarsPeriodType = (BarsPeriodType)101,
                    Value = 100
                };

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
            // Ignore non-trade updates (best approximation)
            if (volume <= 0)
                return;

            if (bars.Count == 0)
            {
                AddBar(bars, close, close, close, close, time, volume);
                tradeCount = 1;
                currentVolume = volume;
                return;
            }

            // Bar complete → new bar starts on THIS trade
            if (tradeCount == bars.BarsPeriod.Value)
            {
                AddBar(bars, close, close, close, close, time, volume);

                tradeCount = 1;
                currentVolume = volume;
                return;
            }

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

        // ✅ REQUIRED OVERRIDES

        public override void ApplyDefaultValue(BarsPeriod barsPeriod)
        {
            barsPeriod.Value = 100;
        }

        public override void ApplyDefaultBasePeriodValue(BarsPeriod barsPeriod)
        {
            barsPeriod.BaseBarsPeriodValue = 1;
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
