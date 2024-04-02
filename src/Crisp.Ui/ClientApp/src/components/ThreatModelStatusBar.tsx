import React, { useState } from 'react';
import { Tooltip, Row, Col } from 'reactstrap';
import ThreatStatus from './threat-model/threats/ThreatStatus';
import { Threat } from '../models/Threat';

interface ThreatModelStatusBarProps {
    id: string;
    threats: Threat[];
}

const ThreatModelStatusBar: React.FC<ThreatModelStatusBarProps> = ({ id, threats }) => {

    const orderedStatuses = [1, 2, 3, 0];

    const [tooltipOpen, setTooltipOpen] = useState(false);

    const totalThreats = threats.length;

    const statusCounts = threats.reduce<{ [key: number]: number }>((acc, threat) => {
        acc[threat.status] = (acc[threat.status] || 0) + 1;
        return acc;
    }, {});

    const getStatusWidth = (status: number): number => {
        return statusCounts[status] ? (statusCounts[status] / totalThreats) * 100 : 0;
    };

    const getStatusColor = (status: number): string => {
        switch (status) {
            case 1: return '#a4262c';   // Not mitigated
            case 2: return '#db7500';   // Partially mitigated
            case 3: return 'green';     // Mitigated
            default: return '#a19f9d';  // Not evaluated
        }
    };

    const toggleTooltip = () => setTooltipOpen(prev => !prev);

    return (
        <>
            <div id={`status-bar-${id}`} style={{ width: '150px', height: '7px', display: 'flex' }}>
                {orderedStatuses.map((status) => (
                    <div
                        key={status}
                        style={{
                            width: `${getStatusWidth(status)}%`,
                            backgroundColor: getStatusColor(status),
                        }}
                    ></div>
                ))}
            </div>
            <Tooltip placement="top" target={`status-bar-${id}`} isOpen={tooltipOpen} toggle={toggleTooltip}>
                <div className="p-2">
                    {orderedStatuses.map(s => statusCounts[s]
                        ? (
                            <Row key={s}>
                                <Col md="10"><ThreatStatus status={s} /></Col>
                                <Col md="2">{statusCounts[s]}</Col>
                            </Row>
                        ) : null
                    )}
                </div>
            </Tooltip>
        </>
    );
};

export default ThreatModelStatusBar;
