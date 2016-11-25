namespace Jering.VectorArtKit.WebApi.ResponseModels.Shared
{
    public class ErrorResponseModel: IErrorResponseModel
    {
        public bool ExpectedError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
