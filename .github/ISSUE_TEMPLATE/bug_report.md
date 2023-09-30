---
name: Bug report
about: Create a report to help us improve
title: "[BUG]"
labels: bug
assignees: vitkuz573

---

## Bug Report

### Prerequisites:
* Please make sure that the bug is not already reported by searching through FlexInject's [issues](https://github.com/vitkuz573/FlexInject/issues).
* Please make sure that you are using the latest version of FlexInject.

### Report Format:

#### Title:
* Provide a descriptive title that includes a concise summary of the issue.

#### Description:
* Provide a more detailed introduction to the issue itself, including any steps to reproduce the bug. A good description saves time and helps in understanding the bug with lesser back-and-forths.

#### Expected Behavior:
* Briefly describe what you expected to happen.

#### Current Behavior:
* Detail what happens instead of the expected behavior.

#### Steps to Reproduce:
1.
2.
3.
...

#### Context:
* Please provide any relevant information about your setup, like the version of FlexInject you are using, and the .NET version.
* This information can be critical in identifying the root cause of the problem.

#### Screenshots:
* If applicable, add screenshots to help explain your problem.

### Example:

```markdown
### Title:
Injection Failure on Scoped Registrations

### Description:
When trying to resolve a scoped service, the container fails and throws an exception. This occurs when calling `container.Resolve<IScopedService>()`.

### Expected Behavior:
The container should resolve the scoped service successfully.

### Current Behavior:
The container throws an InvalidOperationException indicating the service cannot be resolved.

### Steps to Reproduce:
1. Register a scoped service using `container.RegisterScoped<IScopedService, ScopedServiceImplementation>()`.
2. Create a scope using `container.CreateScope()`.
3. Try to resolve the scoped service within the created scope.

### Context:
- FlexInject version: 1.0.0
- .NET version: .NET 6.0

### Screenshots:
[If applicable, add screenshots to help explain your problem.]
```

### Submission:
* Please submit your bug report by creating a new issue on the [GitHub issues page](https://github.com/vitkuz573/FlexInject/issues).

### Note:
* Keep the report focused, and include only one bug per report.
* Be patient, the developers will try to resolve the issue or provide a suitable workaround as soon as possible.
