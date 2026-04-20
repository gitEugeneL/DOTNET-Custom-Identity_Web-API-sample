namespace Api.Services.Interfaces;

public interface IConfirmationService
{
    (string code, DateTime expires) GenerateCode();
}