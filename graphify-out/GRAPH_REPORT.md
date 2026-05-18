# Graph Report - /home/terranova/Documents/development/cicd-orchestrator  (2026-05-18)

## Corpus Check
- Corpus is ~1,299 words - fits in a single context window. You may not need a graph.

## Summary
- 68 nodes · 50 edges · 27 communities (12 shown, 15 thin omitted)
- Extraction: 88% EXTRACTED · 12% INFERRED · 0% AMBIGUOUS · INFERRED: 6 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Core Domain Model|Core Domain Model]]
- [[_COMMUNITY_Server API Endpoints|Server API Endpoints]]
- [[_COMMUNITY_Database Context|Database Context]]
- [[_COMMUNITY_Logs Endpoint|Logs Endpoint]]
- [[_COMMUNITY_Builds Endpoint|Builds Endpoint]]
- [[_COMMUNITY_Runners Endpoint|Runners Endpoint]]
- [[_COMMUNITY_Webhooks Endpoint|Webhooks Endpoint]]
- [[_COMMUNITY_Health Endpoint|Health Endpoint]]
- [[_COMMUNITY_JobCompleted Message|JobCompleted Message]]
- [[_COMMUNITY_JobQueued Message|JobQueued Message]]
- [[_COMMUNITY_Artifact Model|Artifact Model]]
- [[_COMMUNITY_LogMetadata Model|LogMetadata Model]]
- [[_COMMUNITY_Pipeline Model|Pipeline Model]]
- [[_COMMUNITY_Build Model|Build Model]]
- [[_COMMUNITY_Runner Model|Runner Model]]
- [[_COMMUNITY_Job Model|Job Model]]
- [[_COMMUNITY_Webhook Signature Service|Webhook Signature Service]]
- [[_COMMUNITY_Runner Token Service|Runner Token Service]]
- [[_COMMUNITY_Runner Program (semantic)|Runner Program (semantic)]]

## God Nodes (most connected - your core abstractions)
1. `Job` - 8 edges
2. `OrchestratorDbContext` - 7 edges
3. `Server Program` - 6 edges
4. `Build` - 5 edges
5. `JobCompleted` - 3 edges
6. `JobQueued` - 3 edges
7. `Artifact` - 3 edges
8. `Runner` - 3 edges
9. `OrchestratorDbContext` - 2 edges
10. `LogsEndpoints` - 2 edges

## Surprising Connections (you probably didn't know these)
- `JobCompleted` --conceptually_related_to--> `Job`  [INFERRED]
  src/Orchestrator.Contracts/Messages/JobCompleted.cs → src/Orchestrator.Server/Models/Job.cs
- `ArtifactInfo` --conceptually_related_to--> `Artifact`  [INFERRED]
  src/Orchestrator.Contracts/Messages/JobCompleted.cs → src/Orchestrator.Server/Models/Artifact.cs
- `JobCancelled` --conceptually_related_to--> `Job`  [INFERRED]
  src/Orchestrator.Contracts/Messages/JobCancelled.cs → src/Orchestrator.Server/Models/Job.cs
- `JobQueued` --conceptually_related_to--> `Job`  [INFERRED]
  src/Orchestrator.Contracts/Messages/JobQueued.cs → src/Orchestrator.Server/Models/Job.cs
- `Server Program` --references--> `OrchestratorDbContext`  [EXTRACTED]
  src/Orchestrator.Server/Program.cs → src/Orchestrator.Server/Data/OrchestratorDbContext.cs

## Hyperedges (group relationships)
- **Message Contracts** — JobCompleted_cs_JobCompleted, JobCancelled_cs_JobCancelled, JobQueued_cs_JobQueued [INFERRED 0.90]
- **Server Endpoints** — BuildsEndpoints, RunnersEndpoints, LogsEndpoints, WebhooksEndpoints, HealthEndpoints [EXTRACTED 1.00]
- **Domain Models** — Pipeline, Build, Job, Runner, Artifact, LogMetadata [INFERRED 0.90]
- **Status Enums** — BuildStatus, JobStatus, RunnerStatus [INFERRED 0.90]

## Communities (27 total, 15 thin omitted)

### Community 0 - "Core Domain Model"
Cohesion: 0.27
Nodes (11): Artifact, Build, BuildStatus, Job, JobCancelled, JobStatus, LogMetadata, OrchestratorDbContext (+3 more)

### Community 1 - "Server API Endpoints"
Cohesion: 0.25
Nodes (8): BuildsEndpoints, HealthEndpoints, LogsEndpoints, RunnerTokenService, RunnersEndpoints, Server Program, WebhookSignatureService, WebhooksEndpoints

### Community 8 - "JobCompleted Message"
Cohesion: 0.67
Nodes (3): ArtifactInfo, JobCompleted, JobStepResult

### Community 9 - "JobQueued Message"
Cohesion: 0.67
Nodes (3): JobQueued, JobStep, RegistryAuth

## Knowledge Gaps
- **21 isolated node(s):** `Artifact`, `LogMetadata`, `Pipeline`, `Build`, `Runner` (+16 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **15 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `OrchestratorDbContext` connect `Core Domain Model` to `Server API Endpoints`?**
  _High betweenness centrality (0.067) - this node is a cross-community bridge._
- **Why does `Server Program` connect `Server API Endpoints` to `Core Domain Model`?**
  _High betweenness centrality (0.062) - this node is a cross-community bridge._
- **Why does `Job` connect `Core Domain Model` to `JobCompleted Message`, `JobQueued Message`?**
  _High betweenness centrality (0.062) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `Job` (e.g. with `JobQueued` and `JobCompleted`) actually correct?**
  _`Job` has 3 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Artifact`, `LogMetadata`, `Pipeline` to the rest of the system?**
  _21 weakly-connected nodes found - possible documentation gaps or missing edges._