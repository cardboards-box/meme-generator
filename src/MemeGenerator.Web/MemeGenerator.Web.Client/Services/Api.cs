namespace MemeGenerator.Web.Client.Services;

public interface IApi
{
    ITemplatesApiService Templates { get; }

    IMemesApiService Memes { get; }
}

public class Api(
    ITemplatesApiService _templates,
    IMemesApiService _memes) : IApi
{
    public ITemplatesApiService Templates => _templates;

    public IMemesApiService Memes => _memes;
}
