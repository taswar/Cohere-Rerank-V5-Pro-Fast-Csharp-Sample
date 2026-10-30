# Contributing

Thanks for helping improve this sample. It's a small .NET 10 console app that shows how to call Cohere Rerank v5 Pro and Fast on Microsoft Foundry from C#. Contributions that make it clearer, more correct, or more useful to other .NET developers are welcome.

## Ways to contribute

- **Report a bug.** Something fails, or the README doesn't match what the code does.
- **Fix the docs.** Typos, unclear steps, missing troubleshooting cases.
- **Improve the sample.** For example, a Context Focus (`relevant_spans`) example, or a clearer way to tune thresholds.
- **Share results.** If you ran the sample against your own data, an issue describing what you saw helps others set their thresholds.

For anything larger than a small fix, open an issue first so we can agree on the approach before you write code.

## Set up

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Clone the repo and build it:

   ```bash
   git clone https://github.com/taswar/Cohere-Rerank-V5-Pro-Fast-Csharp-Sample.git
   cd Cohere-Rerank-V5-Pro-Fast-Csharp-Sample
   dotnet build
   ```

3. To run it, deploy `Cohere-rerank-v5-pro` and `Cohere-rerank-v5-fast` on a Microsoft Foundry resource and set the endpoints with user secrets, as described in the [README](README.md#configuration). Use an API key, or sign in with `az login` for Entra ID.

## Guidelines

- **Never commit secrets.** No API keys, endpoints, tenant IDs or tokens in code, config files or screenshots. Settings belong in user secrets or environment variables.
- **Keep it a sample.** The code should stay short and readable from top to bottom. Prefer clarity over abstraction, and add a dependency only when it clearly helps.
- **Match the existing style.** Target `net10.0`, keep nullable reference types on, and follow the naming and layout already in `Program.cs`.
- **Keep the README in sync.** If you change behaviour, settings or output, update the README in the same pull request, including the sample output.
- **One change per pull request.** Small, focused pull requests are easier to review and merge.

## Pull request checklist

- [ ] `dotnet build` succeeds with no new warnings.
- [ ] The app runs against your own Foundry deployment, or you explain why you couldn't run it.
- [ ] No secrets, endpoints or personal data are in the diff.
- [ ] The README reflects any change in behaviour, settings or output.
- [ ] The pull request description says what changed and why.

The build workflow runs on every pull request and has to pass before a merge.

## Reporting a bug

Open an issue and include:

- What you ran and what you expected.
- The full error message, with any keys or endpoints removed.
- Your .NET SDK version (`dotnet --version`) and operating system.
- Whether you used an API key or Entra ID.
- The deployment names you used, if they differ from `Cohere-rerank-v5-pro` and `Cohere-rerank-v5-fast`.

## Security issues

Please don't report security problems in a public issue. Use **Security → Report a vulnerability** on the repo to send a private report.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE) that covers this project.
