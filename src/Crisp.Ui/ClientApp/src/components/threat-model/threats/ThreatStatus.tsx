import React from 'react';

const getStatusColor = (status: number): string => {
    switch (status) {
        case 1:  // Not mitigated
            return '#a4262c';
        case 2:  // Partially mitigated
            return '#db7500';
        case 3:  // Mitigated
            return 'green';
        default:
            return '#a19f9d';
    }
};

const getStatusText = (status: number): string => {
    switch (status) {
        case 1:
            return 'Not mitigated';
        case 2:
            return 'Partially mitigated';
        case 3:
            return 'Mitigated';
        default:
            return 'Not evaluated';
    }
}

interface ThreatStatusProps {
    status: number;
}

const ThreatStatus: React.FC<ThreatStatusProps> = ({ status }) => (
    <div style={{ display: 'flex', alignItems: 'center' }}>
        <span style={{
            height: '10px',
            width: '10px',
            backgroundColor: getStatusColor(status),
            borderRadius: '50%',
            display: 'inline-block',
            marginRight: '10px'
        }}></span>
        {getStatusText(status)}
    </div>
);

export default ThreatStatus;
