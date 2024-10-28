namespace WebAPI.Services.Data.Processors;

/// <summary>
/// Provides basic functionality to transform one set of data into another.
/// This is also used to apply business logic over the input data.
/// </summary>
/// <typeparam name="TIn">The input data type.</typeparam>
/// <typeparam name="TOut">The output data type.</typeparam>
public interface IDataProcessor<TIn, TOut> {
    TOut Process(TIn input);
}
