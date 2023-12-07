using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Core.Models;

public record GitHubDirectory(
    string Name,
    string Url,
    IEnumerable<GitHubDirectory>? Directories,
    IEnumerable<GitHubFile>? Files
);
