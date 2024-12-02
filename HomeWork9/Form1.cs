namespace HomeWork9
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int m = int.Parse(textBox1.Text); // Number of samples
            int n = int.Parse(textBox2.Text); // Sample size
            int intervals = int.Parse(textBox3.Text); // Number of intervals
            Random random = new Random();

            // Generate the parent distribution probabilities
            double[] probabilities = GenerateProbabilities(intervals);

            // Calculate theoretical mean and variance
            double theoreticalMean = CalculateMean(probabilities);
            double theoreticalVariance = CalculateVariance(probabilities, theoreticalMean);

            // Collect sampling variances
            List<double> uncorrectedVariances = new List<double>();
            List<double> correctedVariances = new List<double>();

            for (int i = 0; i < m; i++)
            {
                double[] sample = GenerateSample(probabilities, n, intervals, random);

                // Calculate uncorrected and corrected variances for the sample
                double sampleMean = sample.Average();
                double uncorrectedVariance = sample.Select(x => Math.Pow(x - sampleMean, 2)).Sum() / n;
                double correctedVariance = sample.Select(x => Math.Pow(x - sampleMean, 2)).Sum() / (n - 1);

                uncorrectedVariances.Add(uncorrectedVariance);
                correctedVariances.Add(correctedVariance);
            }

            // Analyze the distributions of the variances
            double meanUncorrectedVariance = uncorrectedVariances.Average();
            double varianceUncorrectedVariance = uncorrectedVariances.Select(x => Math.Pow(x - meanUncorrectedVariance, 2)).Average();

            double meanCorrectedVariance = correctedVariances.Average();
            double varianceCorrectedVariance = correctedVariances.Select(x => Math.Pow(x - meanCorrectedVariance, 2)).Average();

            // Display results
            DrawHistogram(uncorrectedVariances, correctedVariances, pictureBox1.Width, pictureBox1.Height, theoreticalVariance);
            DisplayStatistics(meanUncorrectedVariance, varianceUncorrectedVariance, meanCorrectedVariance, varianceCorrectedVariance, theoreticalVariance);
        }

        private double[] GenerateProbabilities(int intervals)
        {
            Random random = new Random();
            double[] probabilities = new double[intervals];
            double sum = 0;

            for (int i = 0; i < intervals - 1; i++)
            {
                double remaining = 1 - sum;
                probabilities[i] = random.NextDouble() * remaining * 0.8;
                sum += probabilities[i];
            }
            probabilities[intervals - 1] = 1 - sum;
            return probabilities;
        }

        private double[] GenerateSample(double[] probabilities, int n, int intervals, Random random)
        {
            double[] sample = new double[n];
            for (int i = 0; i < n; i++)
            {
                double rand = random.NextDouble();
                double cumulative = 0;
                for (int j = 0; j < intervals; j++)
                {
                    cumulative += probabilities[j];
                    if (rand <= cumulative)
                    {
                        sample[i] = j;
                        break;
                    }
                }
            }
            return sample;
        }

        private double CalculateMean(double[] probabilities)
        {
            double mean = 0;
            for (int i = 0; i < probabilities.Length; i++)
                mean += i * probabilities[i];
            return mean;
        }

        private double CalculateVariance(double[] probabilities, double mean)
        {
            double variance = 0;
            for (int i = 0; i < probabilities.Length; i++)
                variance += Math.Pow(i - mean, 2) * probabilities[i];
            return variance;
        }

        private void DrawHistogram(
       List<double> uncorrected,
       List<double> corrected,
       int width,
       int height,
       double theoreticalVariance)
        {
            Graphics graph = pictureBox1.CreateGraphics();
            graph.Clear(Color.White);

            // Combine both uncorrected and corrected variances to find min and max
            List<double> combined = uncorrected.Concat(corrected).ToList();
            double min = combined.Min();
            double max = combined.Max();

            // Number of bins for histogram
            int bins = 20;
            double binWidth = (max - min) / bins;

            // Calculate frequencies
            int[] frequenciesUncorrected = new int[bins];
            int[] frequenciesCorrected = new int[bins];
            foreach (var value in uncorrected)
            {
                int bin = Math.Min((int)((value - min) / binWidth), bins - 1);
                frequenciesUncorrected[bin]++;
            }
            foreach (var value in corrected)
            {
                int bin = Math.Min((int)((value - min) / binWidth), bins - 1);
                frequenciesCorrected[bin]++;
            }

            // Find maximum frequency for scaling
            float maxFrequency = Math.Max(frequenciesUncorrected.Max(), frequenciesCorrected.Max());

            // Bar width and offsets for two histograms
            float barWidth = width / (2 * bins + 4);
            float xOffset = barWidth * 2;
            float yScale = (float)(height / maxFrequency * 0.8); // Scale for better fitting

            // Draw axes
            using (Pen axisPen = new Pen(Color.Black, 2))
            {
                // Y-axis
                graph.DrawLine(axisPen, 0, 0, 0, height);
                // X-axis
                graph.DrawLine(axisPen, 0, height, width, height);
            }

            // Draw histogram bars and percentages
            for (int i = 0; i < bins; i++)
            {
                float xUncorrected = i * 2 * barWidth;
                float xCorrected = xUncorrected + barWidth;

                // Calculate heights
                float heightUncorrected = frequenciesUncorrected[i] * yScale;
                float heightCorrected = frequenciesCorrected[i] * yScale;

                // Draw uncorrected bars in green
                graph.FillRectangle(Brushes.Green, xUncorrected, height - heightUncorrected, barWidth, heightUncorrected);

                // Draw corrected bars in blue
                graph.FillRectangle(Brushes.Blue, xCorrected, height - heightCorrected, barWidth, heightCorrected);

                // Display percentages above bars (omit if percentage is 0.0%)
                using (Font font = new Font("Arial", 8))
                {
                    float percentUncorrected = (frequenciesUncorrected[i] / (float)uncorrected.Count) * 100;
                    float percentCorrected = (frequenciesCorrected[i] / (float)corrected.Count) * 100;

                    if (percentUncorrected > 0.0f)
                    {
                        graph.DrawString($"{percentUncorrected:F1}%", font, Brushes.Black, xUncorrected, height - heightUncorrected - 15);
                    }
                    if (percentCorrected > 0.0f)
                    {
                        graph.DrawString($"{percentCorrected:F1}%", font, Brushes.Black, xCorrected, height - heightCorrected - 15);
                    }
                }
            }

            

            // Add axis labels
            using (Font axisFont = new Font("Arial", 10))
            {
                // Y-axis label
                graph.DrawString("Frequency (%)", axisFont, Brushes.Black, 0, 0);

                // X-axis label
                graph.DrawString("Variance Bins", axisFont, Brushes.Black, width / 2 - 50, height + 20);
            }
        }


        private void DisplayStatistics(double meanUncorrected, double varianceUncorrected, double meanCorrected, double varianceCorrected, double theoreticalVariance)
        {
            string stats = $"Uncorrected Variance:\nMean: {meanUncorrected:F4}, Variance: {varianceUncorrected:F4}\n" +
                           $"Corrected Variance:\nMean: {meanCorrected:F4}, Variance: {varianceCorrected:F4}\n" +
                           $"Theoretical Variance: {theoreticalVariance:F4}";

            MessageBox.Show(stats, "Statistics");
        }


    
    }
}
