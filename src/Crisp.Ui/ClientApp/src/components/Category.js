import React, { useState, useRef, useEffect, useImperativeHandle, forwardRef } from 'react';
import { ListGroupItem, Badge, Input, Button } from 'reactstrap';
import Recommendation from './Recommendation';
import { BsArrowsAngleContract, BsArrowsAngleExpand } from "react-icons/bs";

const Category = forwardRef(({ category, level, isSelected, toggleSelectability }, ref) => {

    const [isCollapsed, setIsCollapsed] = useState(false);
    const [recommendationsExpanded, setRecommendationsExpanded] = useState(false);
    const [recommendationsRefs, setRecommendationsRefs] = useState([]);
    const [categoriesRefs, setCategoriesRefs] = useState([]);

    if (!level) {
        level = 0;
    }

    useEffect(() => {
        setRecommendationsRefs(recommendationsRefs => (
            Array(category.recommendations.length).fill().map((_, i) => recommendationsRefs[i] || React.createRef())
        ));
    }, [category.recommendations.length]);

    useEffect(() => {
        setCategoriesRefs(categoriesRefs => (
            Array(category.children.length).fill().map((_, i) => categoriesRefs[i] || React.createRef())
        ));
    }, [category.children.length]);

    const getPaddingLeft = () => {
        return (level * 30) + 15;
    }

    const calculateRecommendationsCount = (category, onlySelected) => {
        let count = onlySelected
            ? category.recommendations.filter(r => isSelected(r.id)).length
            : category.recommendations.length;
        category.children.forEach(c => count += calculateRecommendationsCount(c, onlySelected));
        return count;
    }

    const toggleIsCollapsed = (e) => {
        if (e.target.tagName === "INPUT") {
            return;
        }
        setIsCollapsed(!isCollapsed);
    }

    const toggleIsSelect = (category) => {
        toggleSelectability(category);
    }

    const expandAllRecommendations = () => {
        setRecommendationsExpanded(true);
        recommendationsRefs.forEach(r => r.current?.open());
        categoriesRefs.forEach(c => c.current?.expandAllRecommendations());
    }

    const collapseAllRecommendations = () => {
        setRecommendationsExpanded(false);
        recommendationsRefs.forEach(r => r.current?.close());
        categoriesRefs.forEach(c => c.current?.collapseAllRecommendations());
    }

    useImperativeHandle(ref, () => ({
        expandAllRecommendations, collapseAllRecommendations
    }));

    const expandAllHandler = (e) => {
        console.log('expandAll');
        expandAllRecommendations();
        e.stopPropagation();
    }

    const collapseAllHandler = (e) => {
        console.log('collapseAll');
        collapseAllRecommendations();
        e.stopPropagation();
    }


    const categorySelected = isSelected(category.id);
    const recommendationsCount = calculateRecommendationsCount(category, false);
    const selectedRecommendationsCount = calculateRecommendationsCount(category, true);

    return (
        <>
            <ListGroupItem className="d-flex justify-content-between align-items-center" style={{ paddingLeft: getPaddingLeft() + 'px' }} action tag="button" onClick={toggleIsCollapsed}>
                <div>
                    <Input className="form-check-input me-3" type="checkbox" checked={categorySelected} onChange={() => toggleIsSelect(category)} />
                    <b>{category.name}</b>
                    {!recommendationsExpanded
                        ? <Button color="link" size="sm" onClick={expandAllHandler} className="ms-3"><BsArrowsAngleExpand /></Button>
                        : <Button color="link" size="sm" onClick={collapseAllHandler} className="ms-3"><BsArrowsAngleContract /></Button>
                    }
                </div>
                <Badge color={categorySelected ? 'primary' : 'secondary'}>
                    {categorySelected ? selectedRecommendationsCount : recommendationsCount}
                </Badge>
            </ListGroupItem>
            {!isCollapsed ? (
                <>
                    {categoriesRefs.map((ref, i) => (
                        <Category key={category.children[i].id} ref={ref} category={category.children[i]} level={level + 1} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    ))}
                    {recommendationsRefs.map((ref, i) => (
                        <Recommendation key={category.recommendations[i].id} ref={ref} recommendation={category.recommendations[i]} level={level + 1} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    ))}
                </>
            ) : null}
        </>
    )
});

export default Category;