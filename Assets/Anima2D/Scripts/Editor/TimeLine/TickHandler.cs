using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    internal class TickHandler
    {
        private int m_BiggestTick = -1;
        private float m_MaxValue = 1f;
        private float m_MinValue;
        private float m_PixelRange = 1f;
        private int m_SmallestTick;
        private float[] m_TickModulos = new float[0];
        private float[] m_TickStrengths = new float[0];

        public int tickLevels => m_BiggestTick - m_SmallestTick + 1;

        public void SetTickModulos(float[] tickModulos)
        {
            m_TickModulos = tickModulos;
        }

        public void SetTickModulosForFrameRate(float frameRate)
        {
            if (frameRate != Mathf.Round(frameRate))
            {
                SetTickModulos(new[]
                {
                    1f / frameRate,
                    5f / frameRate,
                    10f / frameRate,
                    50f / frameRate,
                    100f / frameRate,
                    500f / frameRate,
                    1000f / frameRate,
                    5000f / frameRate,
                    10000f / frameRate,
                    50000f / frameRate,
                    100000f / frameRate,
                    500000f / frameRate
                });
            }
            else
            {
                var list = new List<int>();
                var num = 1;
                while (num < frameRate)
                {
                    if (num == frameRate) break;
                    var num2 = Mathf.RoundToInt(frameRate / num);
                    if (num2 % 60 == 0)
                    {
                        num *= 2;
                        list.Add(num);
                    }
                    else
                    {
                        if (num2 % 30 == 0)
                        {
                            num *= 3;
                            list.Add(num);
                        }
                        else
                        {
                            if (num2 % 20 == 0)
                            {
                                num *= 2;
                                list.Add(num);
                            }
                            else
                            {
                                if (num2 % 10 == 0)
                                {
                                    num *= 2;
                                    list.Add(num);
                                }
                                else
                                {
                                    if (num2 % 5 == 0)
                                    {
                                        num *= 5;
                                        list.Add(num);
                                    }
                                    else
                                    {
                                        if (num2 % 2 == 0)
                                        {
                                            num *= 2;
                                            list.Add(num);
                                        }
                                        else
                                        {
                                            if (num2 % 3 == 0)
                                            {
                                                num *= 3;
                                                list.Add(num);
                                            }
                                            else
                                            {
                                                num = Mathf.RoundToInt(frameRate);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var array = new float[9 + list.Count];
                for (var i = 0; i < list.Count; i++) array[i] = 1f / list[list.Count - i - 1];
                array[array.Length - 1] = 3600f;
                array[array.Length - 2] = 1800f;
                array[array.Length - 3] = 600f;
                array[array.Length - 4] = 300f;
                array[array.Length - 5] = 60f;
                array[array.Length - 6] = 30f;
                array[array.Length - 7] = 10f;
                array[array.Length - 8] = 5f;
                array[array.Length - 9] = 1f;
                SetTickModulos(array);
            }
        }

        public void SetRanges(float minValue, float maxValue, float minPixel, float maxPixel)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_PixelRange = maxPixel - minPixel;
        }

        public float[] GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels)
        {
            var num = Mathf.Clamp(m_SmallestTick + level, 0, m_TickModulos.Length - 1);
            var list = new List<float>();
            var num2 = Mathf.FloorToInt(m_MinValue / m_TickModulos[num]);
            var num3 = Mathf.CeilToInt(m_MaxValue / m_TickModulos[num]);
            for (var i = num2; i <= num3; i++)
                if (!excludeTicksFromHigherlevels || num >= m_BiggestTick ||
                    i % Mathf.RoundToInt(m_TickModulos[num + 1] / m_TickModulos[num]) != 0)
                    list.Add(i * m_TickModulos[num]);
            return list.ToArray();
        }

        public float GetStrengthOfLevel(int level)
        {
            return m_TickStrengths[m_SmallestTick + level];
        }

        public float GetPeriodOfLevel(int level)
        {
            return m_TickModulos[Mathf.Clamp(m_SmallestTick + level, 0, m_TickModulos.Length - 1)];
        }

        public int GetLevelWithMinSeparation(float pixelSeparation)
        {
            for (var i = 0; i < m_TickModulos.Length; i++)
            {
                var num = m_TickModulos[i] * m_PixelRange / (m_MaxValue - m_MinValue);
                if (num >= pixelSeparation) return i - m_SmallestTick;
            }

            return -1;
        }

        public void SetTickStrengths(float tickMinSpacing, float tickMaxSpacing, bool sqrt)
        {
            m_TickStrengths = new float[m_TickModulos.Length];
            m_SmallestTick = 0;
            m_BiggestTick = m_TickModulos.Length - 1;
            for (var i = m_TickModulos.Length - 1; i >= 0; i--)
            {
                var num = m_TickModulos[i] * m_PixelRange / (m_MaxValue - m_MinValue);
                m_TickStrengths[i] = (num - tickMinSpacing) / (tickMaxSpacing - tickMinSpacing);
                if (m_TickStrengths[i] >= 1f) m_BiggestTick = i;
                if (num <= tickMinSpacing)
                {
                    m_SmallestTick = i;
                    break;
                }
            }

            for (var j = m_SmallestTick; j <= m_BiggestTick; j++)
            {
                m_TickStrengths[j] = Mathf.Clamp01(m_TickStrengths[j]);
                if (sqrt) m_TickStrengths[j] = Mathf.Sqrt(m_TickStrengths[j]);
            }
        }
    }
}