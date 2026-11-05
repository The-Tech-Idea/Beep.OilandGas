using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
var root=Path.GetFullPath(args[0]);var source=Directory.GetFiles(Path.Combine(root,"TheTechIdea.Data","OilGas"),"*.cs",SearchOption.AllDirectories);
var trees=source.Select(path=>CSharpSyntaxTree.ParseText(File.ReadAllText(path),new CSharpParseOptions(LanguageVersion.Preview,preprocessorSymbols:new[]{"NET","NET10_0","NETCOREAPP"}),path)).ToArray();
var globals=CSharpSyntaxTree.ParseText("global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq; global using System.Net.Http; global using System.Threading; global using System.Threading.Tasks;",new CSharpParseOptions(LanguageVersion.Preview));
var paths=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Concat(Directory.GetFiles(Path.Combine(root,"TheTechIdea.Data","bin","Debug","net10.0"),"*.dll")).Where(p=>Path.GetFileName(p)!="TheTechIdea.Data.dll").GroupBy(Path.GetFileName,StringComparer.OrdinalIgnoreCase).Select(g=>g.First());
var compilation=CSharpCompilation.Create("DataOwnershipAnalysis",trees.Append(globals),paths.Select(p=>MetadataReference.CreateFromFile(p)),new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,nullableContextOptions:NullableContextOptions.Enable));
var errors=compilation.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).Select(d=>d.ToString()).ToArray();
string Full(INamedTypeSymbol symbol){var parts=new Stack<string>();for(var t=symbol;t is not null;t=t.ContainingType)parts.Push(t.MetadataName);return symbol.ContainingNamespace.ToDisplayString()+"."+string.Join("+",parts);}
var result=new List<object>();
foreach(var tree in trees){var model=compilation.GetSemanticModel(tree);var node=tree.GetRoot();var dependencies=new HashSet<string>();var declarations=new HashSet<string>();
foreach(var typeNode in node.DescendantNodes().OfType<BaseTypeDeclarationSyntax>()){if(model.GetDeclaredSymbol(typeNode) is not INamedTypeSymbol type)continue;declarations.Add(Full(type));foreach(var reference in type.DeclaringSyntaxReferences)if(reference.SyntaxTree.FilePath!=tree.FilePath)dependencies.Add(reference.SyntaxTree.FilePath);}
foreach(var name in node.DescendantNodes().OfType<SimpleNameSyntax>()){var info=model.GetSymbolInfo(name);var symbol=info.Symbol;if(symbol is IAliasSymbol alias)symbol=alias.Target;INamedTypeSymbol? type=symbol switch { INamedTypeSymbol named=>named, IMethodSymbol method=>(method.ReducedFrom??method).ContainingType, IPropertySymbol property=>property.ContainingType, IFieldSymbol field=>field.ContainingType, IEventSymbol evt=>evt.ContainingType, _=>null };if(type is null)continue;foreach(var reference in type.OriginalDefinition.DeclaringSyntaxReferences)if(reference.SyntaxTree.FilePath!=tree.FilePath)dependencies.Add(reference.SyntaxTree.FilePath);}
result.Add(new{Path=tree.FilePath,Declarations=declarations.Order().ToArray(),DependsOn=dependencies.Order().ToArray()});}
File.WriteAllText(args[1],JsonSerializer.Serialize(new{Files=result,Errors=errors},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine($"Bound {trees.Length} current Data files; semantic errors={errors.Length}");foreach(var e in errors.Take(5))Console.WriteLine(e);
