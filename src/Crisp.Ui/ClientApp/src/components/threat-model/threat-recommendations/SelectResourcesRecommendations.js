import React, { useState, useCallback, useMemo } from 'react';
import { Spinner, ListGroup, Alert, Button, Badge } from 'reactstrap';
import { useQuery } from 'react-query';
import { fetchRecommendations } from '../../../fetchers/resources';
import { FiPlus, FiArrowLeft } from "react-icons/fi";
import Category from '../../Category';

const STALE_TIME = 24 * 60 * 60 * 1000;  // 24 hours in milliseconds
const RECOMMENDATION_TITLE_DIVIDER = ' - ';

const SelectResourcesRecommendations = ({ recommendations, resources, onClose }) => {

    const recommendationsCategory = useQuery(['resources-recommendations-category', ...resources], () => fetchRecommendations(resources), { staleTime: STALE_TIME });

    const [selectedList, setSelectedList] = useState([]);

    const getChildrenIds = useCallback(category => {
        let ids = [category.id];
        if (category.children) {
            category.children.forEach(c => ids = [...ids, ...getChildrenIds(c)]);
        }
        if (category.recommendations) {
            category.recommendations.forEach(r => ids = [...ids, r.id]);
        }
        return ids;
    }, []);

    const toggleSelectability = useCallback(selectedCategory => {
        setSelectedList(prev => {
            const toggledIds = getChildrenIds(selectedCategory);
            if (prev.includes(selectedCategory.id)) {
                return prev.filter(id => !toggledIds.includes(id));
            } else {
                return [...prev, ...toggledIds.filter(id => !prev.includes(id))];
            }
        });
    }, [getChildrenIds]);

    const isSelected = useCallback(id => {
        return selectedList.includes(id);
    }, [selectedList]);

    const selectedRecommendations = useMemo(() => {
        const getSelectedRecommendations = (category, titlePrefix = '', isRootCategory = true) => {
            if (!category) {
                return [];
            }

            const updatedTitlePrefix = isRootCategory
                ? titlePrefix
                : titlePrefix ? `${titlePrefix}${RECOMMENDATION_TITLE_DIVIDER}${category.name}` : category.name;

            const parts = updatedTitlePrefix.split(RECOMMENDATION_TITLE_DIVIDER);
            const resourceName = parts[0];
            const categoryNames = parts.slice(1).join(RECOMMENDATION_TITLE_DIVIDER);

            let recommendations = category.recommendations
                .filter(r => isSelected(r.id))
                .map(r => {
                    const title = r.title === "No Related Feature"
                        ? updatedTitlePrefix
                        : updatedTitlePrefix ? `${updatedTitlePrefix}${RECOMMENDATION_TITLE_DIVIDER}${r.title}` : r.title;
                    const titleInDescription = r.title === "No Related Feature"
                        ? ''
                        : `  \n**Title:** ${r.title}`;
                    const description = updatedTitlePrefix
                        ? `**Resource:** ${resourceName}  \n**Category:** ${categoryNames}${titleInDescription}\n\n${r.description}`
                        : r.description;
                    return { ...r, title, description };
                });
            category.children.forEach(c => recommendations = [...recommendations, ...getSelectedRecommendations(c, updatedTitlePrefix, false)]);
            return recommendations;
        }

        return recommendationsCategory?.data ? getSelectedRecommendations(recommendationsCategory.data) : [];
    }, [recommendationsCategory, isSelected]);

    const selectedRecommendationsCount = selectedRecommendations.length;

    const addRecommendations = useCallback(() => {
        onClose(selectedRecommendations);
    }, [onClose, selectedRecommendations]);

    if (recommendationsCategory.isLoading) {
        return <div className="text-center"><Spinner>Loading...</Spinner></div>;
    }

    if (recommendationsCategory.isError) {
        return <Alert color="danger">{recommendationsCategory.error.message}</Alert>;
    }

    return (
        <>
            <div className="mb-3">
                <Button color="secondary" onClick={() => onClose(null)}><FiArrowLeft /> Back to threat</Button>
            </div>
            <h5>Select recommendations</h5>
            {!recommendationsCategory.data ? (
                <p>There are no recommendations</p>
            ) : (
                <>
                    <SelectRecommendationsActionBar onSave={addRecommendations} selectedCount={selectedRecommendationsCount} />
                    <ListGroup flush>
                        <Category category={recommendationsCategory.data} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    </ListGroup>
                    <SelectRecommendationsActionBar onSave={addRecommendations} selectedCount={selectedRecommendationsCount} />
                </>
            )}
        </>
    );
};

const SelectRecommendationsActionBar = ({ onSave, selectedCount }) => (
    <div className="d-flex justify-content-between align-items-center py-2 px-3 border-bottom border-3 border-dark mb-2 mt-2 bg-super-light">
        <Button color="success" onClick={onSave} disabled={selectedCount === 0}><FiPlus /> Add selected recommendations</Button>{' '}
        <span>Selected recommendations <Badge color="primary" className="ms-2 fs-little-smaller">{selectedCount}</Badge></span>
    </div>
);

export default SelectResourcesRecommendations;
