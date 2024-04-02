import React, { useState, useMemo } from 'react';
import { Button, Table, Dropdown, DropdownToggle, DropdownMenu, DropdownItem, Modal, ModalBody } from 'reactstrap';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { FiPlus, FiX } from "react-icons/fi";
import { RxDragHandleDots2 } from "react-icons/rx";
import SelectResourcesRecommendations from './SelectResourcesRecommendations';
import ThreatRecommendation from './ThreatRecommendation';
import SelectSecurityBenchmarkRecommendations from './SelectSecurityBenchmarkRecommendations';
import SelectCatalogRecommendations from './SelectCatalogRecommendations';

const ThreatRecommendations = ({ recommendations, setRecommendations, resources }) => {

    const [addRecommendationMenuOpen, setAddRecommendationMenuOpen] = useState(false);
    const [selectResourcesRecommendationsModalOpen, setSelectResourcesRecommendationsModalOpen] = useState(false);
    const [selectSecurityBenchmarkRecommendationsModalOpen, setSelectSecurityBenchmarkRecommendationsModalOpen] = useState(false);
    const [selectCatalogRecommendationsModalOpen, setSelectCatalogRecommendationsModalOpen] = useState(false);
    const [recommendationModalOpen, setRecommendationModalOpen] = useState(false);
    const [selectedRecommendation, setSelectedRecommendation] = useState(null);

    const sortedRecommendations = useMemo(() => {
        return [...recommendations].sort((a, b) => a.orderIndex - b.orderIndex);
    }, [recommendations]);

    const defaultRecommendation = {
        title: '',
        description: '',
        orderIndex: getNextOrderIndex()
    };

    function getNextOrderIndex() {
        return recommendations.length > 0 ? Math.max(...recommendations.map(t => t.orderIndex)) + 1 : 1;
    }

    const addOrUpdateRecommendation = (recommendation) => {
        setSelectedRecommendation(recommendation);
        setRecommendationModalOpen(true);
    }

    const deleteRecommendation = (e, recommendation) => {
        e.stopPropagation();
        if (!window.confirm(`Do you want to delete recommendation '${recommendation.title}' ?`)) {
            return;
        }
        setRecommendations(prev => prev.filter(t => t.id !== recommendation.id)
            .map(t => t.orderIndex > recommendation.orderIndex ? { ...t, orderIndex: t.orderIndex - 1 } : t));
    }

    const addRecommendationsForResources = () => {
        setSelectResourcesRecommendationsModalOpen(true);
    }

    const addRecommendationsFromSecurityBenchmark = () => {
        setSelectSecurityBenchmarkRecommendationsModalOpen(true);
    }

    const addRecommendationsFromCatalog = () => {
        setSelectCatalogRecommendationsModalOpen(true);
    }

    const toggleAddRecommendationsMenu = () => setAddRecommendationMenuOpen((prevState) => !prevState);

    const toggleSelectResourcesRecommendationsModal = (selectedRecommendations) => {
        if (selectedRecommendations) {
            let orderIndex = getNextOrderIndex();
            const recommendationsToAdd = [];
            selectedRecommendations.filter(r => !recommendations.find(recommendation => recommendation.id === r.id)).forEach(r => {
                recommendationsToAdd.push({
                    id: r.id,
                    title: r.title,
                    description: r.description,
                    orderIndex: orderIndex
                });
                orderIndex++;
            });
            setRecommendations(prev => [...prev, ...recommendationsToAdd]);
        }
        setSelectResourcesRecommendationsModalOpen(prev => !prev);
    }

    const toggleSelectSecurityBenchmarkRecommendationsModal = (selectedRecommendations) => {
        if (selectedRecommendations) {
            let orderIndex = getNextOrderIndex();
            const recommendationsToAdd = [];
            selectedRecommendations.filter(r => !recommendations.find(recommendation => recommendation.id === r.id)).forEach(r => {
                recommendationsToAdd.push({
                    id: r.id,
                    title: r.title,
                    description: r.description,
                    orderIndex: orderIndex
                });
                orderIndex++;
            });
            setRecommendations(prev => [...prev, ...recommendationsToAdd]);
        }
        setSelectSecurityBenchmarkRecommendationsModalOpen(prev => !prev);
    }

    const toggleSelectCatalogRecommendationsModal = (selectedRecommendations) => {
        if (selectedRecommendations) {
            let orderIndex = getNextOrderIndex();
            const recommendationsToAdd = [];
            selectedRecommendations.filter(r => !recommendations.find(recommendation => recommendation.id === r.id)).forEach(r => {
                recommendationsToAdd.push({
                    id: r.id,
                    title: r.title,
                    description: r.description,
                    orderIndex: orderIndex
                });
                orderIndex++;
            });
            setRecommendations(prev => [...prev, ...recommendationsToAdd]);
        }
        setSelectCatalogRecommendationsModalOpen(prev => !prev);
    }

    const toggleRecommendationModal = (recommendation) => {
        if (recommendation) {
            if (recommendation.id) {
                setRecommendations(prev => {
                    const index = prev.findIndex(r => r.id === recommendation.id);
                    prev[index] = recommendation;
                    return [...prev];
                });

                setSelectedRecommendation(null);
            } else {
                recommendation.id = Math.floor(Date.now() / 1000).toString();
                console.log(recommendation.id);
                setRecommendations(prev => [...prev, recommendation]);
            }
        }
        setRecommendationModalOpen(prev => !prev);
    }

    const onDragEnd = (result) => {
        if (!result.destination || result.destination.index === result.source.index) {
            return;
        }

        const recommendationMovedForward = result.destination.index > result.source.index;
        const newOrderIndex = result.destination.index + 1;

        setRecommendations(prev => prev.map(r => {
            if (r.id === result.draggableId) {
                return { ...r, orderIndex: newOrderIndex };
            } else if (recommendationMovedForward) {
                if (r.orderIndex > result.source.index + 1 && r.orderIndex <= newOrderIndex) {
                    return { ...r, orderIndex: r.orderIndex - 1 };
                }
            } else {
                if (r.orderIndex >= newOrderIndex && r.orderIndex < result.source.index + 1) {
                    return { ...r, orderIndex: r.orderIndex + 1 };
                }
            }
            return r;
        }));
    };
    
    return (
        <>
            <div>
                <Dropdown isOpen={addRecommendationMenuOpen} toggle={toggleAddRecommendationsMenu}>
                    <DropdownToggle caret color="success"><FiPlus /> Add recommendation  &nbsp; </DropdownToggle>
                    <DropdownMenu>
                        <DropdownItem onClick={() => addOrUpdateRecommendation(defaultRecommendation)}>New recommendation</DropdownItem>
                        <DropdownItem onClick={addRecommendationsForResources} disabled={resources.length === 0}>For selected resources</DropdownItem>
                        <DropdownItem onClick={addRecommendationsFromSecurityBenchmark}>From cloud security benchmark</DropdownItem>
                        <DropdownItem onClick={addRecommendationsFromCatalog}>From catalog</DropdownItem>
                    </DropdownMenu>
                </Dropdown>
            </div>
            {sortedRecommendations.length > 0 ? (
                <DragDropContext onDragEnd={onDragEnd}>
                    <Droppable droppableId="droppable" direction="vertical">
                        {(provided) => (
                            <Table hover>
                                <thead>
                                    <tr>
                                        <th style={{ width: '70px', minWidth: '70px', maxWidth: '70px' }}></th>
                                        <th>Title</th>
                                        <th style={{ width: '70px', minWidth: '70px', maxWidth: '70px' }}></th>
                                    </tr>
                                </thead>
                                <tbody ref={provided.innerRef} {...provided.droppableProps}>
                                    {sortedRecommendations.map((r, index) => (
                                        <Draggable key={r.id} draggableId={r.id} index={index}>
                                            {(provided, snapshot) => (
                                                <tr
                                                    ref={provided.innerRef}
                                                    {...provided.draggableProps}
                                                    onClick={() => addOrUpdateRecommendation(r)}
                                                    className={`cursor-pointer align-middle ${snapshot.isDragging ? "dragging" : ""}`}
                                                >
                                                    <td {...provided.dragHandleProps} className="drag-handle"><RxDragHandleDots2 /> {r.orderIndex}</td>
                                                    <td><Button color="link" className="text-start">{r.title}</Button></td>
                                                    <td>
                                                        <div className="hstack gap-3 float-end">
                                                            <Button size="sm" outline color="danger" onClick={(e) => deleteRecommendation(e, r)}><FiX /></Button>
                                                        </div>
                                                    </td>
                                                </tr>
                                            )}
                                        </Draggable>
                                    ))}
                                    {provided.placeholder}
                                </tbody>
                            </Table>
                        )}
                    </Droppable>
                </DragDropContext>
            ) : null}
            <Modal isOpen={recommendationModalOpen} toggle={toggleRecommendationModal} fullscreen>
                <ModalBody>
                    <ThreatRecommendation recommendation={selectedRecommendation} onClose={toggleRecommendationModal} />
                </ModalBody>
            </Modal>
            <Modal isOpen={selectResourcesRecommendationsModalOpen} toggle={toggleSelectResourcesRecommendationsModal} fullscreen>
                <ModalBody>
                    <SelectResourcesRecommendations threats={recommendations} setThreats={setRecommendations} onClose={toggleSelectResourcesRecommendationsModal} resources={resources} />
                </ModalBody>
            </Modal>
            <Modal isOpen={selectSecurityBenchmarkRecommendationsModalOpen} toggle={toggleSelectSecurityBenchmarkRecommendationsModal} fullscreen>
                <ModalBody>
                    <SelectSecurityBenchmarkRecommendations onClose={toggleSelectSecurityBenchmarkRecommendationsModal} />
                </ModalBody>
            </Modal>
            <Modal isOpen={selectCatalogRecommendationsModalOpen} toggle={toggleSelectCatalogRecommendationsModal} fullscreen>
                <ModalBody>
                    <SelectCatalogRecommendations onClose={toggleSelectCatalogRecommendationsModal} />
                </ModalBody>
            </Modal>
        </>
    );
};

export default ThreatRecommendations;
