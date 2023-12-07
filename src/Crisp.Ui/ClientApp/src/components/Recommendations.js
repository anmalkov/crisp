import React, { useState } from 'react';
import { Spinner, ListGroup, Alert, Button, Badge } from 'reactstrap';
import { useQuery } from 'react-query';
import { fetchCategory } from '../fetchers/categories';
import Category from './Category';
import { useEffect } from 'react';
import './Recommendations.css';


const Recommendations = () => {

    const { isError, isLoading, data, error } = useQuery(['category'], fetchCategory, { staleTime: 24*60*60*1000 });
    const category = data;

    const [selectedList, setSelectedList] = useState([]);

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

    const isSelected = id => {
        return selectedList.includes(id);
    }

    const getSelectedRecommendations = (category) => {
        if (!category) {
            return [];
        }
        let recommendations = [...category.recommendations.filter(r => isSelected(r.id))];
        category.children.forEach(c => recommendations = [...recommendations, ...getSelectedRecommendations(c)]);
        return recommendations;
    }

    useEffect(() => {
        if (!data) {
            return;
        }
        setSelectedList(getChildrenIds(data));
    }, [data]);

    if (isLoading) {
        return (
            <div className="text-center">
                <Spinner>
                    Loading...
                </Spinner>
            </div>
        );
    }

    if (isError) {
        return (
            <Alert color = "danger">{error.message}</Alert >
        );
    }

    const selectedRecommendations = getSelectedRecommendations(category);
    const selectedRecommendationsCount = selectedRecommendations.length;

    return (
        <div>
            {!category ? (
                <p>There are no recommendations</p>
            ) : (
                <>
                    {selectedRecommendationsCount > 0 ? (
                        <div className="d-flex justify-content-between align-items-center py-2 px-3 border-bottom border-3 border-dark mb-2 bg-super-light">
                            <span>Selected recommendations <Badge color="primary" className="ms-2 fs-little-smaller">{selectedRecommendationsCount}</Badge></span>
                            <Button color="success">Export</Button>
                        </div>
                    ) : null}
                    <ListGroup flush>
                        <Category category={category} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    </ListGroup>
                </>
            )}
        </div>
    );
};

export default Recommendations;