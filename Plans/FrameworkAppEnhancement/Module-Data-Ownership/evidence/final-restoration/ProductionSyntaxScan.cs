using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
var paths=JsonSerializer.Deserialize<string[]>(File.ReadAllText(args[0]))!;var results=new List<object>();
foreach(var path in paths){var root=CSharpSyntaxTree.ParseText(File.ReadAllText(path),new CSharpParseOptions(LanguageVersion.Preview,preprocessorSymbols:new[]{"NET","NET10_0","NETCOREAPP"})).GetRoot();
var ids=root.DescendantNodes().OfType<IdentifierNameSyntax>().Where(n=>!n.Ancestors().Any(a=>a is UsingDirectiveSyntax || a is BaseNamespaceDeclarationSyntax)).Select(n=>n.Identifier.ValueText).Distinct().ToArray();
var ns=root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().Select(n=>n.Name.ToString()).Distinct().ToArray();
var uses=root.DescendantNodes().OfType<UsingDirectiveSyntax>().Where(n=>n.Name is not null).Select(n=>n.Name!.ToString()).Distinct().ToArray();
var types=root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Select(n=>new {Name=n.Identifier.ValueText,Namespace=string.Join(".",n.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(x=>x.Name.ToString())),MetadataName=string.Join("+",n.Ancestors().OfType<TypeDeclarationSyntax>().Reverse().Select(x=>x.Identifier.ValueText+(x.TypeParameterList is null?"":"`"+x.TypeParameterList.Parameters.Count)).Append(n.Identifier.ValueText+(n is TypeDeclarationSyntax td && td.TypeParameterList is not null?"`"+td.TypeParameterList.Parameters.Count:""))),Partial=n.Modifiers.Any(m=>m.RawKind == (int)SyntaxKind.PartialKeyword)}).ToArray();
results.Add(new {Path=path,Identifiers=ids,Namespaces=ns,Usings=uses,Types=types});}
File.WriteAllText(args[1],JsonSerializer.Serialize(results));Console.WriteLine($"Parsed {results.Count} actual C# syntax trees (identifier uses include interpolation expressions, exclude literal text).");
