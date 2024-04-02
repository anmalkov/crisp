import React from 'react';
import { Spinner, Button, Row, Col, Input, FormGroup } from 'reactstrap';
import { useQuery} from 'react-query';
import { fetchResources } from '../../fetchers/resources';
import './ThreatModelResources.css';

const ThreatModelResources = ({ addResourcesRecommendations, setAddResourcesRecommendations, selectedResources, setSelectedResources }) => {

    const resources = useQuery([`fetchResources`], fetchResources, { staleTime: 1 * 60 * 60 * 1000 });

    const onResourcesChange = (resourceName) => {
        if (selectedResources.includes(resourceName)) {
            setSelectedResources(selectedResources.filter(n => n !== resourceName));
        } else {
            setSelectedResources([...selectedResources, resourceName]);
        }
    };

    const resourceButtonColor = (resourceName) => {
        return selectedResources.includes(resourceName) ? 'primary' : 'secondary'
    }

    return (
        <>
            <FormGroup switch>
                <Input className="form-check-input me-3" type="switch" role="switch" checked={addResourcesRecommendations} onChange={() => setAddResourcesRecommendations(!addResourcesRecommendations)} /> Add resources recommendations to threats
            </FormGroup>
            {addResourcesRecommendations ? (
                <Row className="mt-3">
                    {resources.isLoading ? (
                        <div className="text-center">
                            <Spinner>Loading...</Spinner>
                        </div>
                    ) : resources.data.resources.map(r => (
                        <Col key={r}>
                            <Button className="resource-small" onClick={() => onResourcesChange(r)} color={resourceButtonColor(r)}>{r}</Button>
                        </Col>
                    ))}
                </Row>
            ) : null}
        </>
    );
};

export default ThreatModelResources;
