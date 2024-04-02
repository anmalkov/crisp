import { ThreatRecommendation } from "./ThreatRecommendation";

export enum ThreatStatus {
    NotEvaluated,
    NotMitigated,
    PartiallyMitigated,
    Mitigated
}

export enum ThreatRisk {
    NotEvaluated,
    Critical,
    High,
    Medium,
    Low
}
export interface Threat {
    id: string;
    title: string;
    description: string;
    status: ThreatStatus;
    risk: ThreatRisk;
    orderIndex: number;
    recommendations?: ThreatRecommendation[];
    benchmarkIds?: string[];
}
