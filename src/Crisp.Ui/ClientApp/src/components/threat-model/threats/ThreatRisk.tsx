import React from 'react';

const getRiskColor = (risk: number): string => {
    switch (risk) {
        case 1:  // Critical
            return '#a4262c';
        case 2:  // High
            return '#db7500';
        case 3:  // Medium
            return '#ffcb12';
        case 4:  // Low
            return '#0078d4';
        default:
            return '#a19f9d';
    }
};

const getRiskText = (risk: number): string => {
    switch (risk) {
        case 1:
            return 'Critical';
        case 2:
            return 'High';
        case 3:
            return 'Medium';
        case 4:
            return 'Low';
        default:
            return 'Not evaluated';
    }
}

interface ThreatRiskProp {
    risk: number;
}

const ThreatRisk: React.FC<ThreatRiskProp> = ({ risk }) => (
    <div style={{ display: 'flex', alignItems: 'center' }}>
        <span style={{
            height: '20px',
            width: '10px',
            backgroundColor: getRiskColor(risk),
            display: 'inline-block',
            marginRight: '10px'
        }}></span>
        {getRiskText(risk)}
    </div>
);

export default ThreatRisk;
