import React, { useState, useMemo, useCallback, useEffect } from 'react';
import { ListGroupItem, Badge, Input, Button } from 'reactstrap';
import Recommendation from './Recommendation';
import { BsArrowsAngleContract, BsArrowsAngleExpand } from "react-icons/bs";

const Category = ({ category, level = 0, isSelected, toggleSelectability, recommendationsExpanded = false }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);
    const [recommendationsExpandedLocal, setRecommendationsExpandedLocal] = useState(recommendationsExpanded);

    useEffect(() => {
        setRecommendationsExpandedLocal(recommendationsExpanded);
    }, [recommendationsExpanded]);

    const paddingLeft = useMemo(() => (level * 30) + 15, [level]);

    const calculateRecommendationsCount = useCallback((cat, onlySelected) => {
        let count = onlySelected
            ? cat.recommendations.filter(r => isSelected(r.id)).length
            : cat.recommendations.length;
        cat.children?.forEach(c => count += calculateRecommendationsCount(c, onlySelected));
        return count;
    }, [isSelected]);

    const toggleIsCollapsed = useCallback((e) => {
        if (e.target.tagName !== "INPUT") {
            setIsCollapsed(!isCollapsed);
        }
    }, [isCollapsed]);

    const toggleIsSelect = useCallback(() => {
        toggleSelectability(category);
    }, [category, toggleSelectability]);

    const toggleRecommendationsExpanded = useCallback((e) => {
        setRecommendationsExpandedLocal(exp => !exp);
        e.stopPropagation();
    }, []);

    const categorySelected = isSelected(category.id);
    const recommendationsCount = calculateRecommendationsCount(category, false);
    const selectedRecommendationsCount = calculateRecommendationsCount(category, true);

    return (
        <>
            <ListGroupItem className="d-flex justify-content-between align-items-center" style={{ paddingLeft: `${paddingLeft}px` }} action tag="div" onClick={toggleIsCollapsed}>
                <div>
                    <Input className="form-check-input me-3" type="checkbox" checked={categorySelected} onChange={toggleIsSelect} />
                    <b>{category.name}</b>
                    <Button color="link" size="sm" onClick={toggleRecommendationsExpanded} className="ms-3">
                        {recommendationsExpandedLocal ? <BsArrowsAngleContract /> : <BsArrowsAngleExpand />}
                    </Button>
                </div>
                <Badge color={categorySelected ? 'primary' : 'secondary'}>
                    {categorySelected ? selectedRecommendationsCount : recommendationsCount}
                </Badge>
            </ListGroupItem>
            {!isCollapsed && (
                <>
                    {category.children?.map(child => (
                        <Category key={child.id} category={child} level={level + 1} recommendationsExpanded={recommendationsExpandedLocal} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    ))}
                    {category.recommendations.map(recommendation => (
                        <Recommendation key={recommendation.id} recommendation={recommendation} level={level + 1} isOpen={recommendationsExpandedLocal} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    ))}
                </>
            )}
        </>
    );
};

export default Category;
