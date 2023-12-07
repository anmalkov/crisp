import React, { useState, useEffect } from 'react';
import { Spinner, ListGroup, ListGroupItem, Alert, Button, Badge, FormGroup, Label, Input, Row, Col, UncontrolledAlert, CloseButton, Tooltip } from 'reactstrap';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { fetchRecommendations, fetchResources } from '../fetchers/resources';
import { FiEdit2, FiCheck } from "react-icons/fi";
import Category from './Category';
import './Resources.css';

const Resources = () => {

    const [selectedResources, setSelectedResources] = useState([]);
    const [recommendations, setRecommendations] = useState(null);
    const [loadRecommendationsButtonDisabled, setLoadRecommendationsButtonDisabled] = useState(true);
    const [selectedList, setSelectedList] = useState([]);
    const [showResources, setShowResources] = useState(true);

    const resourceNames = useQuery([`fetchResources`], fetchResources, { staleTime: 1 * 60 * 60 * 1000 });

    const queryClient = useQueryClient();

    const getRecommendationsMutation = useMutation(() => {
        return fetchRecommendations(selectedResources);
    });

    useEffect(() => {
        setLoadRecommendationsButtonDisabled(selectedResources.length === 0);
        console.log(selectedResources);
    }, [selectedResources]);

    const changeResourcesHandler = (resourceName) => {
        if (selectedResources.includes(resourceName)) {
            setSelectedResources(selectedResources.filter(n => n !== resourceName));
        } else {
            setSelectedResources([...selectedResources, resourceName]);
        }
    };

    const resourceButtonColor = (resourceName) => {
        return selectedResources.includes(resourceName) ? 'primary' : 'secondary'
    }

    const loadRecommendationsHandler = async () => {
        setRecommendations(await getRecommendationsMutation.mutateAsync());
        setShowResources(false);
        //queryClient.invalidateQueries([`threatmodelreport.${oldThreatModel.id}`]);
    }

    const isSelected = id => {
        return selectedList.includes(id);
    }

    const getChildrenIds = category => {
        let ids = [category.id];
        if (category.children) {
            category.children.forEach(c => ids = [...ids, ...getChildrenIds(c)]);
        }
        if (category.recommendations) {
            category.recommendations.forEach(r => ids = [...ids, r.id]);
        }
        return ids;
    }

    const toggleSelectability = selectedCategory => {
        const toggledIds = getChildrenIds(selectedCategory);
        if (selectedList.includes(selectedCategory.id)) {
            setSelectedList(selectedList.filter(id => !toggledIds.includes(id)));
        } else {
            setSelectedList([...selectedList, ...toggledIds.filter(id => !selectedList.includes(id))]);
        }
    }

    const getSelectedRecommendations = (category) => {
        if (!category) {
            return [];
        }
        let recommendations = [...category.recommendations.filter(r => isSelected(r.id))];
        category.children.forEach(c => recommendations = [...recommendations, ...getSelectedRecommendations(c)]);
        return recommendations;
    }

    const selectedRecommendations = getSelectedRecommendations(recommendations);
    const selectedRecommendationsCount = selectedRecommendations.length;

    if (resourceNames.isLoading) {
        return (
            <div className="text-center">
                <Spinner>
                    Loading...
                </Spinner>
            </div>
        );
    }

    if (resourceNames.isError) {
        return (
            <Alert color="danger">{resourceNames.error.message}</Alert >
        );
    }

    if (!resourceNames.data || resourceNames.data.length === 0) {
        return (
            <p>There are no resources found</p>
        )
    }

    return (
        <>
            {showResources ? (
            <>
                <div className="mb-4">
                    <h4>Resources</h4>
                </div>
                <Row>
                    {resourceNames.data.resources.map(r => (
                        <Col key={r}><Button className="resource" onClick={() => changeResourcesHandler(r)} color={resourceButtonColor(r)}>{r}</Button></Col>
                    ))}
                </Row>
                <FormGroup className="border-top border-3 border-dark pt-3">
                    <Button color="success" onClick={loadRecommendationsHandler} disabled={loadRecommendationsButtonDisabled}><FiCheck /> Load recommendations</Button>
                    {getRecommendationsMutation.isLoading &&
                        <Spinner size="sm">Loading...</Spinner>
                    }
                </FormGroup>
            </>
            ) : (
                <>
                    <FormGroup className="border-top border-3 border-dark pt-3">
                        <Button color="success" onClick={() => setShowResources(true)}><FiEdit2 /> Change resources</Button>
                    </FormGroup>
                    <FormGroup>
                        <div className="mb-4">
                            <h4>Recommendations</h4>
                        </div>
                        {!recommendations ? (
                            <p>There are no recommendations</p>
                        ) : (
                            <>
                                {selectedRecommendationsCount > 0 ? (
                                    <div className="d-flex justify-content-between align-items-center py-2 px-3 border-bottom border-3 border-dark mb-2 bg-super-light">
                                        <span>Selected recommendations <Badge color="primary" className="ms-2 fs-little-smaller">{selectedRecommendationsCount}</Badge></span>
                                    </div>
                                ) : null}
                                <ListGroup flush>
                                    <Category category={recommendations} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                                </ListGroup>
                            </>
                        )}
                    </FormGroup>
                </>
            )}
        </>
    );
};

export default Resources;