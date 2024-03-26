import React, { useState } from 'react';
import { fetchThreatModelCategory } from '../fetchers/threatmodels';
import { Spinner, Alert, Input, Button, Collapse, ListGroup } from 'reactstrap';
import { useQuery } from 'react-query';
import ReactMarkdown from 'react-markdown'
import { fetchBenchmarkControls } from '../fetchers/resources';
import Category from './Category';

import './ThreatMapping.css';

const CategoryItem = ({ category, onRecommendationSelect, selectedRecommendation }) => {
    return (
        <>
            <div>{category.name}</div>
            <div style={{ paddingLeft: '20px' }}>
                {/* Render Sub-Categories */}
                {category.children && category.children.length > 0 && (
                    <div>
                        {category.children.map((subCategory, index) => (
                            <CategoryItem
                                key={index}
                                category={subCategory}
                                onRecommendationSelect={onRecommendationSelect}
                                selectedRecommendation={selectedRecommendation}
                            />
                        ))}
                    </div>
                )}
                {/* Render Recommendations */}
                {category.recommendations && category.recommendations.length > 0 && (
                    <div>
                        {category.recommendations.map((recommendation, index) => (
                            <div
                                key={index}
                                onClick={() => onRecommendationSelect(recommendation)}
                                className={`${selectedRecommendation && selectedRecommendation.id === recommendation.id ? 'selected-recommendation' : ''}`}
                                style={{
                                    cursor: 'pointer',
                                }}
                            >
                                {selectedRecommendation && selectedRecommendation.title === recommendation.title ? "> " : ""}{recommendation.title}
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </>
    );
}

const DescriptionPanel = ({ recommendation }) => {
    if (!recommendation) {
        return <div className="description-panel">Select a recommendation to see its details here.</div>;
    }

    return (
        <>
            <h4 className="mb-4">{recommendation.title}</h4>
            <div className="description-panel">
                <ReactMarkdown>{recommendation.description}</ReactMarkdown>
            </div>
        </>
    );
};

const ThreatMappting = () => {

    const threats = useQuery(['threatmodel-category'], fetchThreatModelCategory, { staleTime: 24 * 60 * 60 * 1000 });
    const benchmarkControls = useQuery(['benchmark-controls-category'], fetchBenchmarkControls, { staleTime: 24 * 60 * 60 * 1000 });

    const [selectedRecommendation, setSelectedRecommendation] = useState(null);
    const [checkedBenchmarkControlIds, setCheckedBenchmarkControlIds] = useState([]);

    const [isThreatsOpen, setIsThreatsOpen] = useState(true);
    
    const handleRecommendationSelect = (recommendation) => {
        if (checkedBenchmarkControlIds.length > 0) {
            if (!window.confirm("Are you sure you want to change the recommendation? All selected benchmark controls will be lost.")) {
                return;
            }
        }
        setSelectedRecommendation(recommendation);
        setCheckedBenchmarkControlIds(recommendation.benchmarkIds || []);
        toggleThreats();
    };

    const toggleThreats = () => {
        setIsThreatsOpen(!isThreatsOpen);
    }

    const getChildrenIds = item => {
        const ids = [item.id];
        if (item.children) {
            item.children.forEach(c => ids.push(...getChildrenIds(c)));
        }
        if (item.recommendations) {
            item.recommendations.forEach(r => ids.push(r.id));
        }
        return ids;
    }
    
    const toggleSelectability = selectedItem => {
        const toggledIds = getChildrenIds(selectedItem);
        setCheckedBenchmarkControlIds(prev => {
            if (prev.includes(selectedItem.id)) {
                return prev.filter(id => !toggledIds.includes(id));
            }
            return [...prev, ...toggledIds.filter(id => !prev.includes(id))];
        });
    }

    const isSelected = id => {
        return checkedBenchmarkControlIds.includes(id);
    }
    function getCsvForIds(ids) {
        return ids.filter(id => id.length <= 10).join(', ');
    }

    if ((threats && threats.isLoading) || (benchmarkControls && benchmarkControls.isLoading)) {
        return (
            <div className="text-center">
                <Spinner>
                    Loading...
                </Spinner>
            </div>
        );
    }

    if ((threats && threats.isError) || (benchmarkControls && benchmarkControls.isError)) {
        return (
            <Alert color="danger">{threats.error.message}</Alert >
        );
    }

    return (
        <>
            <div className="mb-4 d-flex justify-content-between align-items-center">
                <h4>Threats</h4>
                <Button color="primary" onClick={toggleThreats} className="mb-1">Toggle</Button>
            </div>
            <Collapse isOpen={isThreatsOpen}>
                <div>
                    <CategoryItem
                        category={threats.data}
                        onRecommendationSelect={handleRecommendationSelect}
                        selectedRecommendation={selectedRecommendation}
                    />
                </div>
            </Collapse>
            <div className="mt-3">
                <DescriptionPanel recommendation={selectedRecommendation} />
            </div>
            {selectedRecommendation ? (
                <>
                    <div className="mb-4 mt-4">
                        <h4>Cloud Security Benchmark</h4>
                    </div>
                    <ListGroup flush>
                        <Category category={benchmarkControls.data} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    </ListGroup>
                    <div className="mt-4">
                        <h4>Selected Benchmark Controls</h4>
                    </div>
                    <div>
                        <textarea readOnly
                            style={{ width: '100%', height: '50px' }}
                            value={getCsvForIds(checkedBenchmarkControlIds)} />
                    </div>
                </>
            ): null}
        </>
    );
}

export default ThreatMappting;