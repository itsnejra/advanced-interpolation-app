namespace InterpolationApp.Algorithms
{
    /// <summary>
    /// Interface for all interpolation algorithms
    /// </summary>
    public interface IInterpolationAlgorithm
    {
        /// <summary>
        /// Name of the interpolation method
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the algorithm and its characteristics
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Set the data points for interpolation
        /// </summary>
        /// <param name="xPoints">X coordinates (must be sorted and unique)</param>
        /// <param name="yPoints">Y coordinates</param>
        void SetData(double[] xPoints, double[] yPoints);

        /// <summary>
        /// Interpolate value at given x
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <returns>Interpolated y value</returns>
        double Interpolate(double x);

        /// <summary>
        /// Interpolate over a range of x values
        /// </summary>
        /// <param name="xMin">Minimum x value</param>
        /// <param name="xMax">Maximum x value</param>
        /// <param name="numPoints">Number of points to interpolate</param>
        /// <returns>Array of interpolated y values</returns>
        double[] InterpolateRange(double xMin, double xMax, int numPoints);

        /// <summary>
        /// Calculate Root Mean Square Error against test data
        /// </summary>
        /// <param name="testX">Test X values</param>
        /// <param name="testY">Test Y values</param>
        /// <returns>RMSE value</returns>
        double CalculateError(double[] testX, double[] testY);
        
        /// <summary>
        /// Get polynomial equation as a string (if applicable)
        /// </summary>
        /// <returns>String representation of the polynomial equation, or empty string if not applicable</returns>
        string GetPolynomialEquation();
    }
}
