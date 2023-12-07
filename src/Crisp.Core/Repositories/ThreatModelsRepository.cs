using Crisp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Core.Repositories;

public class ThreatModelsRepository : RepositoryBase<ThreatModel>, IThreatModelsRepository
{
    private const string RepositoriesDirectoryName = "data";
    private const string RepositoryFilename = "threatmodels.json";

    public ThreatModelsRepository() : base(RepositoryFilename) { }
}
